# Remote File

The Remote File Orchestrator allows for the remote management of file-based certificate stores. Discovery, Inventory, and Management functions are supported. The orchestrator performs operations by first converting the certificate store into a BouncyCastle PKCS12Store.

#### Integration status: Production - Ready for use in production environments.

## About the Keyfactor Universal Orchestrator Capability

This repository contains a Universal Orchestrator Capability which is a plugin to the Keyfactor Universal Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Universal Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing Capabilities, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific Capability, see below in this readme.

The Universal Orchestrator is the successor to the Windows Orchestrator. This Capability plugin only works with the Universal Orchestrator and does not work with the Windows Orchestrator.




---




## Keyfactor Version Supported

The minimum version of the Keyfactor Universal Orchestrator Framework needed to run this version of the extension is 10.1

## Platform Specific Notes

The Keyfactor Universal Orchestrator may be installed on either Windows or Linux based platforms. The certificate operations supported by a capability may vary based what platform the capability is installed on. The table below indicates what capabilities are supported based on which platform the encompassing Universal Orchestrator is running.
| Operation | Win | Linux |
|-----|-----|------|
|Supports Management Add|&check; |&check; |
|Supports Management Remove|&check; |&check; |
|Supports Create Store|&check; |&check; |
|Supports Discovery|&check; |&check; |
|Supports Renrollment|  |  |
|Supports Inventory|&check; |&check; |


## PAM Integration

This orchestrator extension has the ability to connect to a variety of supported PAM providers to allow for the retrieval of various client hosted secrets right from the orchestrator server itself.  This eliminates the need to set up the PAM integration on Keyfactor Command which may be in an environment that the client does not want to have access to their PAM provider.

The secrets that this orchestrator extension supports for use with a PAM Provider are:

|Name|Description|
|----|-----------|
|ServerUsername|The user id that will be used to authenticate into the server hosting the store|
|ServerPassword|The password that will be used to authenticate into the server hosting the store|
|StorePassword|The optional password used to secure the certificate store being managed|
  

It is not necessary to implement all of the secrets available to be managed by a PAM provider.  For each value that you want managed by a PAM provider, simply enter the key value inside your specific PAM provider that will hold this value into the corresponding field when setting up the certificate store, discovery job, or API call.

Setting up a PAM provider for use involves adding an additional section to the manifest.json file for this extension as well as setting up the PAM provider you will be using.  Each of these steps is specific to the PAM provider you will use and are documented in the specific GitHub repo for that provider.  For a list of Keyfactor supported PAM providers, please reference the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam).



---


﻿<!-- add integration specific information below -->
## Overview
The Remote File Orchestrator Extension is a multi-purpose integration that can remotely manage a variety of file-based certificate stores and can easily be extended to manage others.  The certificate store types that can be managed in the current version are:
- Java Keystores of type JKS
- PKCS12 files, including, but not limited to, Java keystores of type PKCS12
- PEM formatted files
- DER formatted files
- IBM Key Database files (KDB)

While the Keyfactor Universal Orchestrator (UO) can be installed on either Windows or Linux; likewise, the Remote File Orchestrator Extension can be used to manage certificate stores residing on both Windows and Linux servers.  The supported configurations of Universal Orchestrator hosts and managed orchestrated servers are shown below:

| | UO Installed on Windows | UO Installed on Linux |
|-----|-----|------|
|Orchestrated Server on remote Windows server|&check; | |
|Orchestrated Server on remote Linux server|&check; |&check; |
|Orchestrated Server on same server as orchestrator service (Agent)|&check; |&check; |

This orchestrator extension makes use of an SSH connection to communicate remotely with certificate stores hosted on Linux servers and WinRM to communicate with certificate stores hosted on Windows servers.
&nbsp;  
&nbsp;  
## Versioning

The version number of a the Remote File Orchestrator Extension can be verified by right clicking on the RemoteFile.dll file in the Extensions/RemoteFile installation folder, selecting Properties, and then clicking on the Details tab.
&nbsp;  
&nbsp;  

## Security Considerations

**For Linux orchestrated servers:**
1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y" (See "Config File Setup" later in this README for more information on setting up the config.json file). The full list of these commands below:
    * echo
    * find
    * cp
    * rm
    * chown
    * install

2. The Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer (See "Config File Setup" later in this README regarding the config.json file).

**For Windows orchestrated servers:**
1. Make sure that WinRM is set up on the orchestrated server and that the WinRM port is part of the certificate store path when setting up your certificate stores When creating a new certificate store in Keyfactor Command (See "Creating Certificate Stores" later in this README).

2. When creating/configuring a certificate store in Keyfactor Command, you will see a "Change Credentials" link after entering in the destination client machine (IP or DNS).  This link **must** be clicked on to present the credentials dialog.  However, it is not required that you enter separate credentials.  Simply click SAVE in the resulting dialog without entering in credentials to use the credentials that the Keyfactor Orchestrator Service is running under.  Alternatively, you may enter separate credentials into this dialog and use those to connect to the orchestrated server.

Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

**SSH Key-Based Authentiation**
When creating a Keyfactor certificate store for the remote file orchestrator extension (see "Creating Certificate Stores" later in this README, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.
&nbsp;  
&nbsp;  
## Remote File Orchestrator Extension Installation
1. Create the certificate store types you wish to manage.  Please refer to the individual sections devoted to each supported store type under "Certificate Store Types" later in this README.
2. Stop the Keyfactor Universal Orchestrator Service for the orchestrator you plan to install this extension to run on.
3. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
4. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
5. Copy the contents of the download installation zip file to the folder created in Step 3.
6. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values (please see the individual certificate store type sections in "Certificate Store Types" later in this README for more information regarding certificate store types), edit the manifest.json file in the folder you created in step 3, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.  If you created it with the suggested values, this step can be skipped.
7. Modify the config.json file (See the "Configuration File Setup" section later in this README)
8. Start the Keyfactor Universal Orchestrator Service.
&nbsp;  
&nbsp;  
## Configuration File Setup 

The Remote File Orchestrator Extension uses a JSON configuration file.  It is located in the {Keyfactor Orchestrator Installation Folder}\Extensions\RemoteFile.  None of the values are required, and a description of each follows below:  
{  
   "UseSudo": "N",  
   "CreateStoreIfMissing": "N",  
   "UseNegotiate": "N",  
   "SeparateUploadFilePath": "",  
   "FileTransferProtocol":  "SCP",  
   "DefaultLinuxPermissionsOnStoreCreation": "600",  
   "DefaultOwnerOnStoreCreation": ""  
}  

**UseSudo** (Applicable for Linux orchestrated servers only) - Y/N - Determines whether to prefix certain Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. For Windows orchestrated servers, this setting has no effect. Setting this value to "N" will result in "sudo" not being added to Linux commands.  **Default value if missing - N**.  
**CreateStoreOnAddIfMissing** - Y/N - Determines, during a Management-Add job, if a certificate store should be created if it does not already exist.  If set to "N", and the store referenced in the Management-Add job is not found, the job will return an error with a message stating that the store does not exist.  If set to "Y", the store will be created and the certificate added to the certificate store.  **Default value if missing - N**.  
**UseNegotiateAuth** (Applicable for Windows orchestrated servers only) – Y/N - Determines if WinRM should use Negotiate (Y) when connecting to the remote server.  **Default Value if missing - N**.  
**SeparateUploadFilePath**(Applicable for Linux managed servers only) – Set this to the path you wish to use as the location on the orchestrated server to upload/download and later remove temporary work files when processing jobs.  If set to "" or not provided, the location of the certificate store itself will be used.  File transfer itself is performed using SCP or SFTP protocols (see FileT ransferProtocol setting). **Default Value if missing - blank**.  
**FileTransferProtocol** (Applicable for Linux orchestrated servers only) - SCP/SFTP/Both - Determines the protocol to use when uploading/downloading files while processing a job.  Valid values are: SCP - uses SCP, SFTP - uses SFTP, or Both - will attempt to use SCP first, and if that does not work, will attempt the file transfer via SFTP.  **Default Value if missing - SCP**.  
**DefaultLinuxPermissionsOnStoreCreation** (Applicable for Linux managed servers only) - Value must be 3 digits all between 0-7.  The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store (See the "Certificatee Store Types Supported" section later in this README).  **Default Value if missing - 600**.  
**DefaultOwnerOnStoreCreation** (Applicable for Linux managed servers only) - When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner instead.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store (See the "Certificatee Store Types Supported" section later in this README) can override this value for a specific store. **Default Value if missing - blank**.  
&nbsp;  
&nbsp;  
## Certificate Store Types

When setting up the certificate store types you wish the Remote File Orchestrator Extension to manage, there are some common settings that will be the same for all supported types.  To create a new Certificate Store Type in Keyfactor Command, first click on settings (the gear icon on the top right) => Certificate Store Types => Add.  Alternatively, there are CURL scripts for all of the currently implemented certificate store types in the Certificate Store Type CURL Scripts folder in this repo if you wish to automate the creation of the desired store types.

**Common Values:**  
*Basic Tab:*
- **Name** – Required. The display name you wish to use for the new Certificate Store Type.
- **ShortName** - Required. See specific certificate store type instructions below.
- **Custom Capability** - Unchecked
- **Supported Job Types** - Inventory, Add, Remove, Create, and Discovery should all be checked.
- **Needs Server** - Checked
- **Blueprint Allowed** - Checked if you wish to make use of blueprinting.  Please refer to the Keyfactor Command Reference Guide for more details on this feature.
- **Uses PoserShell** - Unchecked
- **Requires Store Password** - Checked.  NOTE: This does not require that a certificate store have a password, but merely ensures that a user who creates a Keyfactor Command Certificate Store MUST click the Store Password button and either enter a password or check No Password.  Certificate stores with no passwords are still possible for certain certificate store types when checking this option.
- **Supports Entry Password** - Unchecked.  

*Advanced Tab:*  
- **Store Path Type** - Freeform
- **Supports Custom Alias** - See specific certificate store type instructions below.
- **Private Key Handling** - See specific certificate store type instructions below
- **PFX Password Style** - Default  

*Custom Fields Tab:*
- **Name:** LinuxFilePermissionsOnStoreCreation, **Display Name:** Linux File Permissions on Store Creation, **Type:** String, **Default Value:** none.  This custom field is **not required**.  If not present, value reverts back to the DefaultLinuxPermissionsOnStoreCreation setting in config.json (see Configuration File Setup section above).  This value, applicable to certificate stores hosted on Linux orchestrated servers only, must be 3 digits all between 0-7.  This represents the Linux file permissions that will be set for this certificate store if created via a Management Create job or a Management Add job where the config.json option CreateStoreOnAddIsMissing is set to "Y".  
- **Name:** LinuxFileOwnerOnStoreCreation, **Display Name:** Linux File Owner on Store Creation, **Type:** String, **Default Value:** none.  This custom field is **not required**.  If not present, value reverts back to the DefaultOwnerOnStoreCreation setting in config.json (see Configuration File Setup section above).  This value, applicable to certificate stores hosted on Linux orchestrated servers only, represents the alternate Linux file owner that will be set for this certificate store if created via a Management Create job or a Management Add job where the config.json option CreateStoreOnAddIsMissing is set to "Y".  Please confirm that the user name associated with this Keyfactor certificate store has valid permissions to chown the certificate file to this owner.

Entry Parameters Tab:
- See specific certificate store type instructions below  

&nbsp;  
&nbsp;  
**************************************
**RFPkcs12 Certificate Store Type**
**************************************

The RFPkcs12 store type can be used to manage any PKCS#12 compliant file format INCLUDING java keystores of type PKCS12.

Use cases supported:
1. One-to-many trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.
3. A mix of trust and key entries.

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFPkcs12**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Required.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- no adittional custom fields/parameters

Entry Parameters Tab:
- no additional entry parameters  

&nbsp;  
CURL script to automate certificate store type creation can be found [here](Certificate%20Store%20Type%20CURL%20Scripts/PKCS12.curl)  

&nbsp;  
&nbsp;  
**************************************
**RFJKS Certificate Store Type**
**************************************

The RFJKS store type can be used to manage java keystores of type JKS.  **PLEASE NOTE:** Java keystores of type PKCS12 **_cannot_** be managed by the RFJKS type.  You **_must_** use RFPkcs12.

Use cases supported:
1. One-to-many trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.
3. A mix of trust and key entries.

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFJKS**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Required.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- no adittional custom fields/parameters

Entry Parameters Tab:
- no additional entry parameters  

&nbsp;  
CURL script to automate certificate store type creation can be found [here](Certificate%20Store%20Type%20CURL%20Scripts/JKS.curl)

&nbsp;  
&nbsp;  
**************************************
**RFPEM Certificate Store Type**
**************************************

The RFPEM store type can be used to manage PEM encoded files.

Use cases supported:
1. Trust stores - A file with one-to-many certificates (no private keys, no certificate chains).
2. Single certificate stores with private key in the file.
3. Single certificate stores with certificate chain and private key in the file.
4. Single certificate stores with private key in an external file.
5. Single certificate stores with certificate chain in the file and private key in an external file 

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFPEM**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Forbidden.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- **Name:** IsTrustStore, **Display Name:** Trust Store, **Type:** Bool, **Default Value:** false.   This custom field is **not required**.  Default value if not present is 'false'.  If 'true', this store will be identified as a trust store.  Any certificates attempting to be added via a Management-Add job that contain a private key will raise an error with an accompanying message.  Multiple certificates may be added to the store in this use case.  If set to 'false', this store can only contain a single certificate with chain and private key.  Management-Add jobs attempting to add a certificate without a private key to a store marked as IsTrustStore = 'false' will raise an error with an accompanying message.
- **Name:** IncludesChain, **Display Name:** Store Includes Chain, **Type:** Bool, **Default Value:** false.   This custom field is **not required**.  Default value if not present is 'false'.  If 'true' the full certificate chain, if sent by Keyfactor Command, will be stored in the file.  The order of appearance is always assumed to be 1) end entity certificate, 2) issuing CA certificate, and 3) root certificate.  If additional CA tiers are applicable, the order will be end entity certificate up to the root CA certificate.  if set to 'false', only the end entity certificate and private key will be stored in this store.  This setting is only valid when IsTrustStore = false.
- **Name:** SeparatePrivateKeyFilePath, **Display Name:** Separate Private Key File Location, **Type:** String, **Default Value:** empty.   This custom field is **not required**. If empty, or not provided, it will be assumed that the private key for the certificate stored in this file will be inside the same file as the certificate.  If the full path AND file name is put here, that location will be used to store the private key as an external file.  This setting is only valid when IsTrustStore = false. 
- **Name:** IsRSAPrivateKey, **Display Name:** Is RSA Private Key, **Type:** Bool, **Default Value:** false.   This custom field is **not required**. Default value if not present is 'false'.  If 'true' it will be assumed that the private key for the certificate is a PKCS#1 RSA formatted private key (BEGIN RSA PRIVATE KEY).  If 'false' (default), encrypted/non-encrypted PKCS#8 (BEGIN [ENCRYPTED] PRIVATE KEY) is assumed  If set to 'true' the store password **must** be set to "no password", as PKCS#1 does not support encrypted keys.  This setting is only valid when IsTrustStore = false. 

Entry Parameters Tab:
- no additional entry parameters

&nbsp;  
CURL script to automate certificate store type creation can be found [here](Certificate%20Store%20Type%20CURL%20Scripts/PEM.curl)

&nbsp;  
&nbsp;  
**************************************
**RFDER Certificate Store Type**
**************************************

The RFDER store type can be used to manage DER encoded files.

Use cases supported:
1. Single certificate stores with private key in an external file.
5. Single certificate stores with no private key. 

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFDER**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Forbidden.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- **Name:** SeparatePrivateKeyFilePath, **Display Name:** Separate Private Key File Location, **Type:** String, **Default Value:** empty.   This custom field is **not required**. If empty, or not provided, it will be assumed that there is no private key associated with this DER store.  If the full path AND file name is entered here, that location will be used to store the private key as an external file in DER format. 

Entry Parameters Tab:
- no additional entry parameters

&nbsp;  
CURL script to automate certificate store type creation can be found [here](Certificate%20Store%20Type%20CURL%20Scripts/DER.curl)

&nbsp;  
&nbsp;  
**************************************
**RFKDB Certificate Store Type**
**************************************

The RFKDB store type can be used to manage IBM Key Database Files (KDB) files.  The IBM utility, GSKCAPICMD, is used to read and write certificates from and to the target store and is therefore required to be installed on the server where the Keyfactor Orchestrator Service is installed, and its location MUST be in the system $Path.

Use cases supported:
1. One-to-many trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.
3. A mix of trust and key entries.

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFKDB**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Required.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- no adittional custom fields/parameters

Entry Parameters Tab:
- no additional entry parameters  

&nbsp;  
CURL script to automate certificate store type creation can be found [here](Certificate%20Store%20Type%20CURL%20Scripts/KDB.curl)  

&nbsp;  
&nbsp;  
## Creating Certificate Stores and Scheduling Discovery Jobs

Please refer to the Keyfactor Command Reference Guide for information on creating certificate stores and scheduling Discovery jobs in Keyfactor Command.  However, there are a few fields that are important to highlight here - Client Machine, Store Path (Creating Certificate Stores), and Directories to search (Discovery jobs) and Extensions (Discovery jobs).  For Linux orchestrated servers, "Client Machine" should be the DNS or IP address of the remote orchestrated server while "Store Path" is the full path and file name of the file based store, beginning with a forward slash (/).  For Windows orchestrated servers, "Client Machine" should be of the format {protocol}://{dns-or-ip}:{port} where {protocol} is either http or https, {dns-or-ip} is the DNS or IP address of the remote orchestrated server, and {port} is the port where WinRM is listening, by convention usually 5985 for http and 5986 for https.  "Store Path" is the full path and file name of the file based store, beginning with a drive letter (i.e. c:\).  For example valid values for Client Machine and Store Path for Linux and Windows managed servers may look something like:  

Linux: Client Machine - 127.0.0.1 or MyLinuxServerName; Store Path - /home/folder/path/storename.ext  
Windows: Client Machine - http<span>s://My.Server.Domain:59</span>86; Store Path - c:\folder\path\storename.ext  

For "Directories to search", you can chain paths with a comma delimiter as documented in the Keyfactor Command Reference Guide, but there is also a special value that can be used instead - fullscan.  Entering fullscan in this field will tell the RemoteFile discovery job to search all available drive letters and recursively search all of them for files matching the other search criteria.  

For "Extensions", a reserved value of noext will cause the RemoteFile discovery job to search for files that do not have an extension.  This value can be chained with other extensions using a comma delimiter.  For example, entering pem,jks,noext will cause the RemoteFile discovery job to search for files with extensions of PEM or JKS or files that do not have extensions.  
&nbsp;  
&nbsp;  
## Developer Notes

The Remote File Orchestrator Extension is meant to be extended to be used for other file based certificate store types than the ones referenced above.  The advantage to extending this integration rather than creating a new one is that the configuration, remoting, and Inventory/Management/Discovery logic is already written.  The developer needs to only implement a few classes and write code to convert the destired file based store to a common format.  This section describes the steps necessary to add additional store/file types.  Please note that familiarity with the [.Net Core BouncyCastle cryptography library](https://github.com/bcgit/bc-csharp) is a prerequisite for adding additional supported file/store types.  

Steps to create a new supported file based certificate store type:

1. Clone this repository from GitHub
2. Open the .net core solution in the IDE of your choice
3. Under the ImplementationStoreTypes folder, create a new folder named for the new certificate store type
4. Create a new class (with namespace of Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}) in the new folder that will implement ICertificateStoreSerializer.  By convention, {StoreTypeName}CertificateSerializer would be a good choice for the class name.  This interface requires you to implement two methods: DesrializeRemoteCertificateStore and SerializeRemoteCertificateStore.  The first method will be called passing in a byte array containing the contents of file based store you are managing.  The developer will need to convert that to an Org.BouncyCastle.Pkcs.Pkcs12Store class and return it.  The second method takes in an Org.BouncyCastle.Pkcs.Pkcs12Store and converts it to a collection of custom file representations, List<SerializedStoreInfo>.  This is where the majority of the development will be done.
5. Create an Inventory.cs class (with namespace of Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}) under the new folder and have it inherit InventoryBase.  Override the internal GetCertificateStoreSerializer() method with a one line implementation returning a new instantiation of the class created in step 4.
6. Create a Management.cs class (with namespace of Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}) under the new folder and have it inherit ManagementBase.  Override the internal GetCertificateStoreSerializer() method with a one line implementation returning a new instantiation of the class created in step 4.
7. Modify the manifest.json file to add three new sections (for Inventory, Management, and Discovery).  Make sure for each, the "NewType" in Certstores.{NewType}.{Operation}, matches what you will use for the certificate store type short name in Keyfactor Command.  On the "TypeFullName" line for all three sections, make sure the namespace matches what you used for your new classes.  Note that the namespace for Discovery uses a common class for all supported types.  Discovery is a common implementation for all supported store types.
8. After compiling, move all compiled files, including the config.json and manifest.json to {Keyfactor Orchestrator Installation Folder}\Extensions\RemoteFile.
9. Create the certificate store type in Keyfactor Command
10. Add a new CURL script to build the proper Keyfactor Command certificate store type and place it under "Certificate Store Type CURL Scripts".  The name of the file should match the ShortName you are using for the new store type.
11. Update the documenation in readme_source.md by adding a new section under "Certificate Store Types" for this new supported file based store type.  Include a pointer to the CURL script created in step 10.
&nbsp;  
&nbsp;  
## License
[Apache](https://apache.org/licenses/LICENSE-2.0)


