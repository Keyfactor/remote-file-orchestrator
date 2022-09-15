# Remote File

The Remote File Orchestrator allows for the remote management of file-based certificate stores. Discovery, Inventory, and Management functions are supported. The orchestrator performs operations by first converting the certificate store into a BouncyCastle PKCS12Store.

#### Integration status: Pilot - Ready for use in test environments. Not for use in production.

## About the Keyfactor Universal Orchestrator Capability

This repository contains a Universal Orchestrator Capability which is a plugin to the Keyfactor Universal Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Universal Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing Capabilities, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific Capability, see below in this readme.

The Universal Orchestrator is the successor to the Windows Orchestrator. This Capability plugin only works with the Universal Orchestrator and does not work with the Windows Orchestrator.

---



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



---

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
- Name: linuxFilePermissionsOnStoreCreation, Display Name: Linux File Permissions on Store Creation, Type: String, Default Value: none....This custom field is **required**.

Entry Parameters Tab:
- See specific certificate store type instructions below  
&mbsp;
&nbsp;
**PKCS12 Certificate Store Type**

The PKCS12 store type can be used to manage any PKCS#12 compliant file format.

Use cases supported:
1. Trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. Key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.

Specific Certificate Store Type Values 
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
&nbsp;  
**JKS Certificate Store Type**

The JKS store type can be used to manage java keystores of type jks.

Use cases supported:
1. Trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. Key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.

Specific Certificate Store Type Values 
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
&nbsp;  

![](Images/image5.png)

  - **Separate Private Key (Name MUST be "separatePrivateKey"):** Select if the store will contain a private key but the private key will reside in an separate file somewhere else on the server

![](Images/image3.png)

  - **Private Key Path (Name MUST be "pathToPrivateKey"):** If the certificate store has a separate private key file, this is the FULL PATH and file name where the private key resides. File paths on Linux servers will always begin with a "/". Windows servers will always begin with the drive letter, colon, and backslash, such as "c:\\".

![](Images/image4.png)

  - **Linux File Permissions on Store Creation (Name MUST be "linuxFilePermissionsOnStoreCreation"):** - Optional parameter. Overrides the optional config.json DefaultLinuxPermissionsOnStoreCreation setting (see section 4 below) for a specific certificate store. This value will set the file permissions (Linux only) of a new certificate store created via a Management-Create job. If this parameter is not added or added but not set, the permissions used will be derived from the DefaultLinuxPermissionsOnStoreCreation setting.

![](Images/image8.png)


**2. Register the PEMChain Orchestrator Extension with Keyfactor**

Follow the instructions in the PEMChain Extension Installation section above.  After, navigate to Keyfactor Command => Orchestrators => Management.  If the orchestrator you just installed the extension for has a status of "New", right click on it and select "Approve">

**3a. (Optional) Create a PEMChain Certificate Store within Keyfactor Command**

If you choose to manually create a PEMChain store In Keyfactor Command rather than running a Discovery job to automatically find the store, you can navigate to Certificate Locations =\> Certificate Stores within Keyfactor Command to add the store. Below are the values that should be entered.

![](Images/image6.png)

- **Category** – Required. The PEMChain type name must be selected.
- **Container** – Optional. Select a container if utilized.
- **Client Machine & Credentials** – Required. The server name or IP Address and login credentials for the server where the Certificate Store is located.  The credentials for server login can be any of:

  - UserId/Password

  - UserId/SSH private key.  If using a SSH private key, the following formats are supported:
    - RSA in OpenSSL PEM and ssh.com format
    - DSA in OpenSSL PEM and ssh.com format
    - ECDSA 256/384/521 in OpenSSL PEM format
    - ECDSA 256/384/521, ED25519 and RSA in OpenSSH key format

  - PAM provider information to pass the UserId/Password or UserId/SSH private key credentials

  If managing a PEMChain certificate store on a Windows server, the format of the machine name must be – http://ServerName:5985, where &quot;5985&quot; is the WinRM port number. 5985 is the standard, but if your organization uses a different port, use that.  The Keyfactor Command service account will be used if the credentials are left blank.  **However, if you choose to not enter credentials and use the Keyfactor Command service account, it is required that the *Change Credentials* link still be clicked on and the resulting dialog closed by clicking OK.**
- **Store Path** – Required. The FULL PATH and file name of the PEMChain store being managed. File paths on Linux servers will always begin with a "/". Windows servers will always begin with the drive letter, colon, and backslash, such as "c:\\".  Valid characters for Linux store paths include any alphanumeric character, space, forward slash, hyphen, underscore, and period. For Windows servers, the aforementioned characters as well as a colon and backslash.
- **Separate Private Key File** – Check if the store has a separate private key file.
- **Path to Private Key File** – If Separate Private Key File is checked, enter the FULL PATH to the private key file. File paths on Linux servers will always begin with a "/". Windows servers will always begin with the drive letter, colon, and backslash, such as "c:".
- **Linux File Permissions on Store Creation** - Optional (Linux only). Set the Linux file permissions you wish to be set when creating a new physical certificate store via checking Create Certificate Store above.  This value must be 3 digits all betwwen 0-7.  
- **Orchestrator** – Select the orchestrator you wish to use to manage this store
- **Store Password** – Required. Set the store password or set no password after clicking the supplied button.  If a store password is entered, this value will be used when encrypting private keys that get written to the certificate store during certificate add operations.  Selecting "No Password" will cause an unencrypted private key to be saved during add operations.
- **Create Certificate Store** - Check before saving if you wish to schedule a Management-Create job to physically create the certificate store on the target server.
- **Inventory Schedule** – Set a schedule for running Inventory jobs or none, if you choose not to schedule Inventory at this time.

**3b. (Optional) Schedule a PEMChain Discovery Job**

Rather than manually creating PEMChain certificate stores, you can schedule a Discovery job to search an orchestrated server and find them.

First, in Keyfactor Command navigate to Certificate Locations =\> Certificate Stores. Select the Discover tab and then the Schedule button. Complete the dialog and click Done to schedule.

![](Images/image7.png)

- **Category** – Required. The PEM SSH type name must be selected.
- **Orchestrator** – Select the orchestrator you wish to use to manage this store
- **Client Machine & Credentials** – Required. The server name or IP Address and login credentials for the server where the Certificate Store is located.  The credentials for server login can be any of:

  - UserId/Password

  - UserId/SSH private key.  If using a SSH private key, the following formats are supported:
    - RSA in OpenSSL PEM and ssh.com format
    - DSA in OpenSSL PEM and ssh.com format
    - ECDSA 256/384/521 in OpenSSL PEM format
    - ECDSA 256/384/521, ED25519 and RSA in OpenSSH key format

  - PAM provider information to pass the UserId/Password or UserId/SSH private key credentials

  When setting up a Windows server, the format of the machine name must be – http://ServerName:5985, where &quot;5985&quot; is the WinRM port number. 5985 is the standard, but if your organization uses a different port, use that.  The Keyfactor Command service account will be used if the credentials are left blank.  **However, if you choose to not enter credentials and use the Keyfactor Command service account, it is required that the *Change Credentials* link still be clicked on and the resulting dialog closed by clicking OK.**
- **When** – Required. The date and time when you would like this to execute.
- **Directories to search** – Required. A comma delimited list of the FULL PATHs and file names where you would like to recursively search for PEMChain stores. File paths on Linux servers will always begin with a "/". Windows servers will always begin with the drive letter, colon, and backslash, such as "c:\\".  Entering the string "fullscan" when Discovering against a Windows server will automatically do a recursive search on ALL local drives on the server.
- **Directories to ignore** – Optional. A comma delimited list of the FULL PATHs that should be recursively ignored when searching for PEMChain stores. Linux file paths will always begin with a "/". Windows servers will always begin with the drive letter, colon, and backslash, such as "c:\\".
- **Extensions** – Optional but suggested. A comma delimited list of the file extensions (no leading "." should be included) the job should search for. If not included, only files in the searched paths that have **no file extension** will be returned. If providing a list of extensions, using "noext" as one of the extensions will also return files with no file extension. For example, providing an Extensions list of "pem, noext" would return all file locations within the paths being searched with a file extension of "pem" and files with no extensions.
- **File name patterns to match** – Optional. A comma delimited list of full or partial file names (NOT including extension) to match on.  Use "\*" as a wildcard for begins with or ends with.  Example: entering "ab\*, \*cd\*, \*ef, ghij" will return all stores with names that _**begin with**_ "ab" AND stores with names that _**contain**_ "cd" AND stores with names _**ending in**_ "ef" AND stores with the _**exact name**_ of "ghij".
- **Follow SymLinks** – NOT IMPLEMENTED. Leave unchecked.
- **Include PKCS12 Files** – NOT IMPLEMENTED. Leave unchecked.

Once the Discovery job has completed, a list of PEMChain store locations should show in the Certificate Stores Discovery tab in Keyfactor Command. Right click on a store and select Approve to bring up a dialog that will ask for the Keystore Password. Enter the store password, click Save, and the Certificate Store should now show up in the list of stores in the Certificate Stores tab.

From the Certificate Store list, edit the newly added store to enter whether the store has a separate private key file, and if necessary, the FULL PATH to that file. **NOTE:** You will not be able to successfully process an Inventory or Management job for this store until this has been completed.

***

### License
[Apache](https://apache.org/licenses/LICENSE-2.0)


