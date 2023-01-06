#!/bin/bash

#if [[ $(id -u) -ne 0 ]]; then
#    echo 'Elevation is required to run this script'
#    exit 1
#fi

OPTIONS=fv
LONGOPTS=help,verbose,force,source:,destination:,in-place,service-suffix:,service-user:,no-service,orchestrator-name:,url:,username:,password:,what-if,capabilities:,client-auth-certificate:,client-auth-certificate-password:
PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")

eval set -- "$PARSED"

scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Set default parameters
force=false
verbose=false
sourceDir="$scriptDir"
inPlace=false
noService=false
whatIf=false
capabilities='default'
MINIMUM_SUPPORTED_DOTNET_VERSION="3.1.0";

# Relative Paths
orchestratorDllRelative='./Orchestrator.dll'
appSettingsRelative='./configuration/appsettings.json'
credsRelative='./configuration/orchestratorsecrets.json'
serviceTemplateRelative='./template.service'
logsRelative='./logs'
scriptsRelative='./Scripts'

# Display helpful information
display_help() 
{
    echo "[Required] --url - URL of the Keyfactor server. (https://mywebserver01.keyexample.com/KeyfactorAgents)"

    echo "[Optional] --help"
    echo "[Optional] --username - The username that will be used to connect to Keyfactor. (<domain>\\\\<user>)"
    echo "[Optional] --password - The password that will be used to connect to Keyfactor."
    echo "[Optional] -v, --verbose"
    echo "[Optional] -f, --force - If specified, installation will warn and continue on potential conflicts."
    echo "[Optional] --source - Directory containing the files to install. Defaults to the directory containing this script."
    echo "[Optional] --destination - Directory where the orchestrator will be installed. Defaults to /opt/keyfactor/orchestrator."
    echo "[Optional] --in-place - If specified, the files will be configured in their current location."
    echo "[Optional] --service-suffix - Suffix of the service name, following 'KeyfactorOrchestrator-'."
    echo "[Optional] --service-user - The user that the service will run as. Defaults to 'keyfactor-orchestrator'."
    echo "[Optional] --no-service - If specified, no service will be created. The orchestrator can be manually run in the future."
    echo "[Optional] --orchestrator-name - Identifier used by the Keyfactor server. Defaults to the current machine's hostname."
    echo "[Optional] --capabilities {all|none|ssl} - If specified, installation will include only the specified capabilities. Defaults to all except SSL Sync."
    echo "[Optional] --client-auth-certificate - If specified, this certificate will be used to authenticate to the Keyfactor Command server. (.p12 format expected)"
    echo "[Optional] --client-auth-certificate-password - The password for the certificate specified in --client-auth-certificate"
    echo "[Optional] --what-if - Displays any errors that would arise during installation without making any changes to the server."

    exit 1 ;
}

check_dotnet_version()
{
    inputVersion="$1"

    systemMajorVersion=$(echo "$inputVersion" | awk -F "." '{print $1}')
    systemMinorVersion=$(echo "$inputVersion" | awk -F "." '{print $2}')
    systemPatchVersion=$(echo "$inputVersion" | awk -F "." '{print $3}')

    minimumMajorVersion=$(echo "$MINIMUM_SUPPORTED_DOTNET_VERSION" | awk -F "." '{print $1}')
    minimumMinorVersion=$(echo "$MINIMUM_SUPPORTED_DOTNET_VERSION" | awk -F "." '{print $2}')
    minimumPatchVersion=$(echo "$MINIMUM_SUPPORTED_DOTNET_VERSION" | awk -F "." '{print $3}')

    greaterMajorVersion=$([ "$systemMajorVersion" -gt "$minimumMajorVersion" ] && echo "true" || echo "false")
    equalMajorVersion=$([ "$systemMajorVersion" -eq "$minimumMajorVersion" ] && echo "true" || echo "false")
    greaterMinorVersion=$([ "$systemMinorVersion" -gt "$minimumMinorVersion" ] && echo "true" || echo "false")
    equalMinorVersion=$([ "$systemMinorVersion" -eq "$minimumMinorVersion" ] && echo "true" || echo "false")
    greaterEqualPatchVersion=$([ "$systemPatchVersion" -ge "$minimumPatchVersion" ] && echo "true" || echo "false")

    if [ $greaterMajorVersion = 'true' ] || ([ $equalMajorVersion = 'true' ] && [ $greaterMinorVersion = 'true' ]) || ([ $equalMajorVersion = 'true' ] && [ $equalMinorVersion = 'true' ] && [ $greaterEqualPatchVersion = 'true' ])
    then
        minimumSupportedVersionFound=true;
    fi

    echo $minimumSupportedVersionFound
}

# Remove an existing Keyfactor Orchestrator service
remove_orchestrator_service()
{
    serviceToRemove=$1

    if [[ -z $serviceToRemove ]]; then
        return
    fi

    echo "Removing existing service ${serviceToRemove%.*}"
#    systemctl stop "$serviceToRemove"
#    systemctl disable "$serviceToRemove"

    rm "/etc/systemd/system/$serviceToRemove" &> /dev/null
}

# extract options and their arguments into variables.
while true ; do
    case "$1" in
        -f|--force)
            force=true ; shift ;;
        -v|--verbose)
            verbose=true ; shift ;;
        --help)
            display_help ;;
        --source)
            sourceDir="$(readlink -m "$2")" ; shift 2 ;;
        --destination)
            destinationDir="$(readlink -m "$2")" ; shift 2;;
        --in-place)
            inPlace=true ; shift ;;
        --service-suffix)
            serviceName="$2" ; shift 2 ;;
        --service-user)
            serviceUser="$2" ; shift 2 ;;
        --no-service)
            noService=true ; shift ;;
        --orchestrator-name)
            orchestratorName="$2" ; shift 2 ;;
        --url)
            url="$2" ; shift 2 ;;
        --username)
            username="$2" ; shift 2 ;;
        --password)
            password="$2" ; shift 2 ;;
        --capabilities)
            capabilities="$2" ; shift 2 ;;
        --client-auth-certificate-password)
            clientAuthCertificatePassword="$2" ; shift 2 ;;
        --client-auth-certificate)
            clientAuthCertificate="$2" ; shift 2 ;;
        --what-if)
            whatIf=true ; shift ;;
        --) shift ; break ;;
        *) echo "Unexpected argument $1. Run with --help for assistance." ; exit 1 ;;
    esac
done

# Validate parameters and set defaults
# ensure some form of authentication method was supplied
if [[ -z "$username" ]] && [[ -z "$clientAuthCertificate" ]]; then 
    echo "No method of authentication was provided. Please provide either --username or --client-auth-certificate"
#    exit 1;
fi

# ensure no more than one method of authentication was supplied
if [[ -n "$username" ]] && [[ -n "$clientAuthCertificate" ]]; then 
    echo "Cannot specify both --username and --client-auth-certificate"
#    exit 1;
fi

# if using credentials, ensure both a username and password was supplied
if [[ -n "$username" ]] && [[ -z "$password" ]]; then 
    echo "Must specify --password with --username"
#    exit 1;
fi

# if using client auth, ensure both a certificate and password was supplied
if [[ -n "$clientAuthCertificate" ]] && [[ -z "$clientAuthCertificatePassword" ]]; then 
    echo "Must specify --client-auth-certificate-password with --client-auth-certificate"
#    exit 1;
fi

# if client auth is being used, ensure client certificate can be found
if [[ -n "$clientAuthCertificate" ]] && [[ ! -f "$clientAuthCertificate" ]]; then 
    echo "Unable to find a certificate at $clientAuthCertificate"
#    exit 1;
fi

case "$capabilities" in 
    all|none|ssl|default)
        ;;
    *)
        echo "Invalid value supplied for --capabilities. Possible values include 'all', 'none', and 'ssl'" ; exit ;;
esac

if $inPlace; then
    if [[ "$destinationDir" ]]; then
        echo "Cannot specify both --in-place and --destination"; exit 1 ;
    else
        destinationDir="$sourceDir"
    fi
else
    if [[ -z "$destinationDir" ]]; then
        destinationDir='/opt/keyfactor/orchestrator'
    fi

    if [[ "$sourceDir" = "$destinationDir" ]]; then
        echo "Source and destination directories are both $sourceDir. Use --in-place if you don't want to copy files"; exit 1 ;
    fi
fi    

if $noService; then
    if [[ "$serviceName" ]]; then
        echo "Cannot specify both --no-service and --service-suffix"; exit 1 ;
    fi
    
    if [[ "$serviceUser" ]]; then
        echo "Cannot specify both --no-service and --service-user"; exit 1 ;
    fi
else
    if [[ -z "$serviceName" ]]; then
        serviceName='keyfactor-orchestrator-default'
        serviceDescription='Keyfactor Orchestrator (default)'
    elif [[ ! "$serviceName" =~ ^[A-Za-z0-9-]+$ ]]; then
        echo "--service-suffix has invalid value $serviceName"; exit 1 ;
    else
        serviceName="keyfactor-orchestrator-$serviceName"
        serviceDescription="Keyfactor Orchestrator ($serviceName)"
    fi    
    
    if [[ -z "$serviceUser" ]]; then
        serviceUser='keyfactor-orchestrator'
    elif [[ ! "$serviceUser" =~ ^[A-Za-z0-9-]+$ ]]; then
        echo "--service-user has invalid value $serviceUser"; exit 1 ;
    fi    
fi

if [[ -z "$orchestratorName" ]]; then
    orchestratorName="$( uname -n )"
elif [[ ! "$orchestratorName" =~ ^[A-Za-z0-9\._-]+$ ]]; then
    echo "--orchestrator-name has invalid value $orchestratorName"; exit 1 ;
fi

if [[ -z "$url" ]]; then
    echo "--url is a required parameter" ; exit 1 ;
fi

if $inPlace && [ "$capabilities" != 'all' ]; then 
    echo "--in-place may only be used with capability suite 'all'" ; exit 1 ;
fi

# Print final parameter values
if $verbose; then
    echo "Force: $force"
    echo "Verbose: $verbose"
    echo "Source: $sourceDir"
    echo "Destination: $destinationDir"
    echo "In Place: $inPlace"
    echo "Service Name: $serviceName"
    echo "Service user: $serviceUser"
    echo "No Service: $noService"
    echo "Orchestrator name: $orchestratorName"
    echo "URL: $url"
    echo "Username: $username"
    echo "Client Auth Certificate: $clientAuthCertificate"
    echo "Capabilities: $capabilities"
    echo "What If: $whatIf"
fi

# Environmental checks (set additional vars for action later)
checkSuccess=true
createUser=false
minimumSupportedVersionFound=false;
overwriteDirectoryLinkedServices=false
overwriteService=false
overwriteAppSettings=false

if [[ ! $(type -P 'dotnet') ]]; then
    echo "dotnet could not be found on the PATH. Please ensure that the .NET Core Runtime version $MINIMUM_SUPPORTED_DOTNET_VERSION or later is installed"; 
    checkSuccess=false;
fi

if [[ $(type -P 'dotnet') ]]; then
    for dotnetVersion in $(dotnet --list-runtimes | grep 'Microsoft.NETCore.App' | awk '{print $2}') 
    do   
        minimumSupportedVersionFound="$(check_dotnet_version "$dotnetVersion")"
        if [ $minimumSupportedVersionFound = 'true' ]
        then
            break
        fi
    done
fi

if ! $minimumSupportedVersionFound; then
    echo ".NET Core Runtime version $MINIMUM_SUPPORTED_DOTNET_VERSION or later was not found. Please ensure this runtime is installed"; 
    checkSuccess=false;
fi

if [[ ! $(type -P 'jq') ]]; then
    echo 'jq could not be found on the PATH. Please ensure that the jq utility is installed'; checkSuccess=false ;
fi

if [[ ! $(type -P 'curl') ]]; then
    echo 'curl could not be found on the PATH. Please ensure that the curl utility is installed'; checkSuccess=false ;
fi

if [[ ! -f "$sourceDir/$orchestratorDllRelative" ]]; then
    echo "Source path $sourceDir does not contain the Keyfactor Orchestrator"; checkSuccess=false ;
fi

$verbose && echo "Testing connection to $url"
if [ -n "$username" ]; then
    curlHttp=$(curl --write-out "%{http_code}" --output /dev/null --silent --user "$username:$password" "$url/Session/Status")
    curlError=$?
fi

if [ -n "$clientAuthCertificate" ]; then 
    curlHttp=$(curl --write-out "%{http_code}" --output /dev/null --silent --cert "$clientAuthCertificate" --cert-type p12 --pass "$clientAuthCertificatePassword" "$url/Session/Status")
    curlError=$?
fi

if [[ $curlError -ne 0 ]]; then
    echo "curl request to $url exited with code $curlError"; checkSuccess=false ;
else
    case "$curlHttp" in
        200)
            $verbose && echo "URL test for $url succeeded" ;;
        401)
            echo "URL test for $url failed with 401 Unauthorized."
            checkSuccess=false 
            ;;
        *)
            echo "URL test for $url failed with HTTP status $curlHttp"
            checkSuccess=false 
            ;;
    esac
fi

#Validate the JSON of the app settings file
if ! jq '.' "$sourceDir/$appSettingsRelative" > /dev/null ; then
    if $force; then
        $verbose && echo "appsettings.json is missing or invalid, and will be replaced"
        overwriteAppSettings=true
    else
        echo "appsettings.json is missing or invalid. Use --force if you would like to create a new appsettings.json file"; checkSuccess=false ;
    fi
fi

if ! $noService; then
    # Check for template file
    if [[ ! -f "$sourceDir/$serviceTemplateRelative" ]]; then
        echo "Source path $sourceDir does not contain the systemd service template"; checkSuccess=false ;
    fi

    # Check for existing services
#    if [[ $(systemctl list-unit-files --type service | grep -Eo "^$serviceName.service") ]]; then
#        if $force; then
#            $verbose && echo "Service $serviceName already exists and will be overwritten"
#
#        else
#            echo "Service $serviceName already exists. Use --force if you would like this service to be overwritten"; checkSuccess=false ;
#        fi
#    else
#        $verbose && echo "Service $serviceName will be created"
#    fi
    overwriteService=true

    # Check for non-existent user
    if ! (id "$serviceUser" &> /dev/null); then
        if $force; then
            $verbose && echo "User $serviceUser does not exist and will be created"
            createUser=true
        else
            echo "User $serviceUser does not exist. Use --force if you would like the user to be created"; checkSuccess=false ;
        fi
    else
        $verbose && echo "User $serviceUser already exists"
    fi
fi  

# Check for existing services linked to the installation directory
for f in $(ls -a /etc/systemd/system/keyfactor-orchestrator-* 2> /dev/null); do
    if [ ! -z $(cat $f | grep "WorkingDirectory=$destinationDir") ]; then
        foundServiceFileName=$(basename $f)
        foundServiceName=${foundServiceFileName%.*}

        # Already matched by name, continue
        if ! $noService && [[ "$foundServiceName" == "$serviceName" ]]; then
            continue
        fi

        if $force; then
            $verbose && echo "Service $foundServiceName already exists in this directory and will be removed"
            overwriteDirectoryLinkedServices=true
        else
            echo "Service $foundServiceName already exists in this directory. Use --force if you would like this service to be removed"; checkSuccess=false ;
        fi
    fi
done

if ! $inPlace; then
    # Check for files being overwritten
    if [[ -e "$destinationDir" ]] && [[ "$(ls -A "$destinationDir")" ]]; then
        if $force; then
            $verbose && echo "Directory $destinationDir is not empty. Files will be overwritten"
        else
            echo "Directory $destinationDir is not empty. Use --force if you would like to overwrite this directory"; checkSuccess=false ;
        fi
    fi
fi

# Exit if the checks failed, or if we were just validating
#$checkSuccess || exit 2;

$whatIf && exit 0;

# Do the installation
if $createUser; then
    echo "Creating user $serviceUser"
    adduser --disabled-password --gecos "" "$serviceUser"
fi

$verbose && echo "Creating log directory"
! $whatIf && mkdir -p "$sourceDir/$logsRelative"
$verbose && echo "Creating scripts directory"
! $whatIf && mkdir -p "$sourceDir/$scriptsRelative"

if $overwriteService; then
    remove_orchestrator_service "$serviceName.service"
fi

if $overwriteDirectoryLinkedServices; then
    # Loop through all of the keyfactor-orchestrator-* service files in /etc/systemd/system/
    for kfOrchestratorFile in $(ls -a /etc/systemd/system/keyfactor-orchestrator-* 2> /dev/null); do
        # String match the WorkingDirectory of the service to the installation directory
        if [ ! -z $(cat $kfOrchestratorFile | grep -E '^WorkingDirectory='"$destinationDir/?"'$') ]; then
            remove_orchestrator_service $(basename $kfOrchestratorFile)
        fi
    done
fi

if ! $inPlace; then
    # calculate capabilities that should be included
    capabilityList=(
        "CertStores.FTP.Inventory"
        "CertStores.FTP.Management"
        "Custom.FetchLogs"
    )

    case "$capabilities" in 
        ssl) 
            unset capabilityList
            capabilityList=(
                "SSL.Discovery"
                "SSL.Monitoring"
            ) 
            
            ;;
        none)
            unset capabilityList  
        
            ;;
        all)
            capabilityList+=('SSL.Discovery')
            capabilityList+=('SSL.Monitoring')

            ;;
    esac

    mkdirParams=()
    $verbose && mkdirParams+=(-v)
    mkdir -p "${mkdirParams[@]}"  "$destinationDir"

    if [[ -e "$destinationDir" ]] && [[ "$(ls -A "$destinationDir")" ]]; then
        $verbose && echo "Removing existing files from $destinationDir"
        rm -r "$destinationDir"/*
    fi

    echo "Copying files from $sourceDir to $destinationDir"
    rsync -r "${mkdirParams[@]}" "$sourceDir"/* "$destinationDir" --exclude "extensions/"
    mkdir -p "${mkdirParams[@]}" "$destinationDir/extensions"

    # copy capabilities in accordance with what the user has provided
    for capability in "${capabilityList[@]}";
    do 
        case "$capability" in
            CertStores.FTP*)
                if [ ! -d "$destinationDir/extensions/FTP" ]; then
                    cp -r "$sourceDir/extensions/FTP" "$destinationDir/extensions/FTP"
                fi;;

            SSL.*)
                if [ ! -d "$destinationDir/extensions/SSL" ]; then
                    cp -r "$sourceDir/extensions/SSL" "$destinationDir/extensions/SSL"
                fi;;

            Custom.FetchLogs)
                if [ ! -d "$destinationDir/extensions/FetchLogs" ]; then
                    cp -r "$sourceDir/extensions/FetchLogs" "$destinationDir/extensions/FetchLogs"
                fi;;
        esac
    done

    # the generic extensions need copied in order for the orchestrator to support any "powershell" jobs
    if [ ! -d "$destinationDir/extensions/JobExtensionDrivers" ]; then
        cp -r "$sourceDir/extensions/JobExtensionDrivers" "$destinationDir/extensions/JobExtensionDrivers"
    fi
fi

pushd "$destinationDir" > /dev/null

echo "Saving app settings"
if $overwriteAppSettings; then
    appSettingsContent='{}'
else
    appSettingsContent="$(cat "$appSettingsRelative")"
fi

appSettingsContent="$(echo "$appSettingsContent" | jq --arg cert "$clientAuthCertificate" '.AppSettings.CertPath = $cert')"
appSettingsContent="$(echo "$appSettingsContent" | jq --arg orch "$orchestratorName" '.AppSettings.OrchestratorName = $orch')"
appSettingsContent="$(echo "$appSettingsContent" | jq --arg serverUri "$url" '.AppSettings.AgentsServerUri = $serverUri')"
echo "$appSettingsContent" > "$appSettingsRelative"

credsContent='{}'
credsContent="$(echo "$credsContent" | jq --arg cp "$clientAuthCertificatePassword" '.Secrets.ClientAuthCertificatePassword = $cp')"
credsContent="$(echo "$credsContent" | jq --arg un "$username" '.Secrets.Username = $un')"
credsContent="$(echo "$credsContent" | jq --arg pw "$password" '.Secrets.Password = $pw')"
echo "$credsContent" > "$credsRelative"

echo "Setting file permissions"
chmod -R ug+w "$logsRelative"
chmod 0600 "$credsRelative"

if ! $noService; then
    chown -R "$serviceUser:" .

    sed -i "s/^Description=.*$/Description=$serviceDescription/" "$serviceTemplateRelative"
    sed -i "s/^SyslogIdentifier=.*$/SyslogIdentifier=$serviceUser/" "$serviceTemplateRelative"
    sed -i "s/^User=.*$/User=$serviceUser/" "$serviceTemplateRelative"
    sed -i "s/^Group=.*$/Group=$serviceUser/" "$serviceTemplateRelative"
    # ; as delimiter here since we are replacing paths, which will have / in them
    sed -i "s;^WorkingDirectory=.*$;WorkingDirectory=$destinationDir;" "$serviceTemplateRelative"
    sed -i "s;^ExecStart=.*$;ExecStart=$(type -P dotnet) $orchestratorDllRelative;" "$serviceTemplateRelative"

    echo "Installing systemd service $serviceName"
    cp "$serviceTemplateRelative" "/etc/systemd/system/$serviceName.service"
#    systemctl enable "$serviceName.service"

    echo "Starting systemd service $serviceName"
#    systemctl start "$serviceName.service"
    echo "Start service manually $(type -P dotnet) $orchestratorDllRelative;\" \"$serviceTemplateRelative\""
fi

popd > /dev/null
