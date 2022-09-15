<!-- add integration specific information below -->
## Overview
The Remote File Orchestrator Extension is a multi-purpose integration that can remotely manage a variety of file-based certificate stores and can easily be extended to manage others.  The certificate store types that can be managed in the current version are:
- Java Keystores of type JKS
- PKCS12 files, including, but not limited to, Java keystores of type PKCS12
- PEM files

While the Keyfactor Universal Orchestrator (UO) can be installed on either Windows or Linux, likewise, the Remote File Orchestrator Extension can be used to manage certificate stores residing on both Windows and Linux servers.  The supported configurations of Universal Orchestrator hosts and managed orchestrated servers are shown below:

| | UO Installed on Windows | UO Installed on Linux |
|-----|-----|------|
|Orchestrated Server on remote Windows server|&check; | |
|Orchestrated Server on remote Linux server|&check; |&check; |
|Orchestrated Server on same server as orchestrator service (Agent)|&check; |&check; |

This orchestrator extension makes use of an SSH connection to communicate remotely with certificate stores hosted on Linux servers and WinRM to communicate with certificate stores hosted on Windows servers.
&nbsp;  
&nbsp;  
## Versioning

The version number of a the Remote File Orchestrator Extension can be verified by right clicking on the n the Extensions/RemoteFile installation folder, selecting Properties, and then clicking on the Details tab.
&nbsp;  
&nbsp;  
## Keyfactor Version Supported

The Remote File Orchestrator Extension has been tested against Keyfactor Universal Orchestrator version 9.5, but should work against earlier or later versions of the Keyfactor Universal Orchestrator.
&nbsp;  
&nbsp;  
## Security Considerations

**For Linux orchestrated servers:**
1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with will need elevated access to run these commands, you must set the id up as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y" (See section "Config File Setup" later in this README regarding the config.json file). The full list of these commands below:
    * echo
    * find
    * cp
    * rm
    * chown
    * install

2. The Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer (See section "Config File Setup" later in this README regarding the config.json file).

**For Windows orchestrated servers:**
1. Make sure that WinRM is set up on the orchestrated server and that the WinRM port is part of the certificate store path when setting up your certificate stores (See "Certificate Store Setup" for each supported certificate store type later in this README). 

2. When creating/configuring a certificate store in Keyfactor Command, you will see a "Change Credentials" link after entering in the destination client machine (IP or DNS).  This link **must** be clicked on to present the credentials dialog.  However, WinRM does not require that you enter separate credentials.  Simply click SAVE in the resulting dialog without entering in credentials to use the credentials that the Keyfactor Orchestrator Service is running under.  Alternatively, you may enter separate credentials into this dialog and use those to connect to the orchestrated server.

**SSH Key-Based Authentiation**
1. When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or you can use a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are SUPPORTED.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.
&nbsp;  
&nbsp;  
## Remote File Orchestrator Extension Installation
1. Create the certificate store types you wish to manage.  Please refer to the individual sections devoted to each supported store type under "Certificate Store Types" later in this README.
2. Stop the Keyfactor Universal Orchestrator Service for the orchestrator you plan to install this extension to run on.
3. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator), find the "extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
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
   "DefaultLinuxPermissionsOnStoreCreation": "600"  
}  

**UseSudo** (Applicable for Linux orchestrated servers only) - Y/N - Determines whether to prefix certain Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. For Windows orchestrated servers, this setting has no effect. Setting this value to "N" will result in "sudo" not being added to Linux commands.  **Default value - N**.  
**CreateStoreOnAddIfMissing** - Y/N - Determines, during a Management-Add job, if a certificate store should be created if it does not already exist.  If set to "N", and the store referenced in the Management-Add job is not found, the job will return an error with a message stating that the store does not exist.  If set to "Y", the store will be created and the certificate added to the certificate store.  **Default value - N**.  
<span style="color:red">**UseNegotiateAuth**</span> (Applicable for Windows orchestrated servers only) – Y/N - Determines if WinRM should use Negotiate (Y) when connecting to the remote server.  **Default Value - N**.  
**SeparateUploadFilePath**(Applicable for Linux managed servers only) – Set this to the path you wish to use as the location on the orchestrated server to upload and later remove temporary work files when processing jobs.  If set to "" or not provided, the location of the certificate store itself will be used.  File transfer itself is performed using SCP or SFTP protocols (see FileT ransferProtocol setting). **Default Value - blank**.  
**FileTransferProtocol** (Applicable for Linux orchestrated servers only) - SCP/SFTP/Both - Determines the protocol to use when uploading/downloading files while processing a job.  Valid values are: SCP - uses SCP, SFTP - uses SFTP, or Both - will attempt to use SCP first, and if that does not work, will attempt the file transfer via SFTP.  **Default Value - SCP**.  
**DefaultLinuxPermissionsOnStoreCreation** (Applicable for Linux managed servers only) - Value must be 3 digits all between 0-7.  The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store (See the "Certificatee Store Types Supported" secgtion later in this README).  **Default Value - 600**.
&nbsp;  
&nbsp;  
## Certificate Store Types

When setting up the certificate store types you wish the Remote File Orchestrator Extension to manage, there are some common settings that will be the same for all supported types.  To create a new Certificate Store Type in Keyfactor Command, first click on settings (the gear icon on the top right) => Certificate Store Types => Add.

**Common Values:**  
*Basic Tab:*
- **Name** – Required. The display name you wish to use for the new Certificate Store Type.
- **ShortName** - Required. See specific certificate store type instructions below.
- **Custom Capability** - Unchecked
- **Supported Job Types** - Inventory, Add, Remove, Create, and Discovery should all be checked.
- **Needs Server** - Checked
- **Blueprint Allowed** - Checked if you wish to mske use of blueprinting.  Pleaes refer to the Keyfactor Command Reference Guide for more details on this feature.
- **Uses PoserShell** - Unchecked
- **Requires Store Password** - Checked.  NOTE: This does not require that a certificate store have a password, but merely ensures that a user who creates a Keyfactor Command Certificate Store MUST click the Store Password button and either enter a password or check No Password.  Certificate stores with no passwords are still possible for certain certificate store types when checking this option.
- **Supports Entry Password** - See specific certificate store type instructions below.  

*Advanced Tab:*  
- **Store Paty Type** - Freeform
- **Supports Custom Alias** - See specific certificate store type instructions below.
- **Private Key Handling** - See specific certificate store type instructions below
- **PFX Password Style** - Default  

*Custom Fields Tab:*
- **Name:** linuxFilePermissionsOnStoreCreation, **Display Name:** Linux File Permissions on Store Creation, **Type:** String, **Default Value:** none....This custom field is **not required**.  If not present, value reverts back to DefaultLinuxPermissionsOnStoreCreation setting in config.json (see Configuration File Setup section above).  This value, applicable to certificate stores hosted on Linux orchestrated servers only, must be 3 digits all between 0-7.  This represents the Linux file permissions that will be set for this certificate store if created via a Management Create job or a Management Add job where the config.json option CreateStoreOnAddIsMissing is set to "Y".  

Entry Parameters Tab:
- See specific certificate store type instructions below  

&nbsp;  
**PKCS12 Certificate Store Type**

The PKCS12 store type can be used to manage any PKCS#12 compliant file format.

Use cases supported:
1. Trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. Key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.

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
**JKS Certificate Store Type**

The JKS store type can be used to manage java keystores of type jks.

Use cases supported:
1. Trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. Key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.

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
**PEM Certificate Store Type**

The PEM store type can be used to manage PEM encoded files.

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
- **Name:** IsTrustStore, **Display Name: Trust Store**, **Type:** Bool, **Default Value:** false.   This custom field is **not required**.  Default value if not present is 'false'.  If 'true', this store will be identified as a trust store.  Any certificates attempting to be added via a Management-Add job that contain a private key will raise an error with an accompanying message.  Multiple certificates may be added to the store in this use case.  If set to 'false', this store can only contain a single certificate with chain and private key.  Management-Add jobs attempting to add a certificate without a private key to a store marked as IsTrustStore = 'false' will raise an error with an accompanying message.
- **Name:** IncludesChain, **Display Name:** Store Includes Chain, **Type:** Bool, **Default Value:** false.   This custom field is **not required**.  Default value if not present is 'false'.  If 'true' the full certificate chain, if sent by Keyfactor Command, will be stored in the file.  The order of appearance is always assumed to be 1) end entity certificate, 2) issuing CA certificate, and 3) root certificate.  If additional CA tiers are applicable, the order will be end entity certificate up to the root CA certificate.  if set to 'false', only the end entity certificate and private key will be stored in this store.  This setting is only valid when IsTrustStore = false.
- **Name:** SeparatePrivateKeyFilePath, **Display Name:** Separate Private Key File Location, **Type:** String, **Default Value:** empty.   This custom field is **not required**. If empty, or not provided, it will be assumed that the private key for the certificate stored in this file will be inside the same file as the certificate.  If the full path AND file name is put here, that location will be used to store the private key as an external file.  This setting is only valid when IsTrustStore = false. 

Entry Parameters Tab:
- no additional entry parameters
### License
[Apache](https://apache.org/licenses/LICENSE-2.0)

