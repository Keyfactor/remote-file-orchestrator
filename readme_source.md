<!-- add integration specific information below -->
## Overview
The Remote File Orchestrator Extension is a multi-purpose integration that can remotely manage a variety of file-based certificate stores and can easily be extended to manage others.  The certificate store types that can be managed in the current version are:
- Java Keystores of type JKS
- PKCS12 files, including, but not limited to, Java keystores of type PKCS12
- PEM files

## Versioning

The version number of a the Remote File Orchestrator Extension can be verified by right clicking on the n the Extensions/RemoteFile installation folder, selecting Properties, and then clicking on the Details tab.

## Keyfactor Version Supported

The Remote File Orchestrator Extension has been tested against Keyfactor Universal Orchestrator version 9.5, but should work against earlier or later versions of the Keyfactor Universal Orchestrator.

## Remote File Orchestrator Extension Installation
1. Create the certificate store types you wish to manage.  Please refer to the individual sections devoted to each supported store type later in this README.
2. Stop the Keyfactor Universal Orchestrator Service for the orchestrator you plan to install this extension to run on.
3. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator), find the "extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
4. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
5. Copy the contents of the download installation zip file to the folder created in Step 2.
6. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values (please see the individual certificate store type sections later in this README for more information regarding that), edit the manifest.json file in the ../extensions/RemoteFile folder and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.  If you created it with the suggested values, this step is not necessary.
7. Start the Keyfactor Universal Orchestrator Service.

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


## PEMChain Orchestrator Configuration

**1. Create the New Certificate Store Type for the New PEMChain Orchestrator Extension**

In Keyfactor Command create a new Certificate Store Type similar to the one below:

![](Images/image1.png)
![](Images/image2.png)

- **Name** – Required. The display name of the new Certificate Store Type
- **Short Name** – Required. **MUST** match the folder name under Extensions in the installation folder.  Default="**PEMChain**"
- **Custom Capability** - Unchecked.
- **Supported Job Types** - Inventory, Add, Remove, Create, and Discovery should all be checked.
- **Needs Server, Blueprint Allowed, Uses Powershell, Requires Store Password, Supports Entry Password** – All checked/unchecked as shown
- **Store PathType** – Freeform (user will enter the location of the store)
- **Supports Custom Alias** – Required. Select Forbidden. Aliases are not used for PEMChain stores.
- **Private Keys** – Required (a certificate in a PEMChain certificate **must** have a private key.  No trust store use case available.)
- **PFX Password Style** – Select Default.
 
- **Custom Parameters** – Three custom parameters are used for this store type. They are:

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

**4. Update Settings in config.json**

The PEMChain Orchestrator uses a JSON config file:

{  
"UseSudo": "N",  
"CreateStoreOnAddIfMissing": "N",  
"UseSeparateUploadFilePath": "N",  
"SeparateUploadFilePath": "/path/to/upload/folder/",  
"UseNegotiateAuth": "N",  
"UseSCP": "N",  
"DefaultLinuxPermissionsOnStoreCreation": "600"  
}

**UseSudo** (Applicable for Linux managed servers only) - Y/N - Determines whether to prefix certain Linux command with "sudo". This can be very helpful in ensuring that the user id running commands ssh uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. For orchestrated Windows servers, this setting has no effect. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
**CreateStoreOnAddIfMissing** - Y/N - Determines if during a Management-Add job if a certificate store should be created if it does not already exist.  If set to "N", the job will return an error with a message stating that the store does not exist.  
**UseSeparateUploadFilePath** (Applicable for Linux managed servers only) – When adding a certificate to a PEM or PKCS12 store, the PEMChain Orchestrator must upload the certificate being deployed to the server where the certificate store resides. Setting this value to "Y" looks to the next setting, SeparateUploadFilePath, to determine where this file should be uploaded. Set this value to "N" to use the same path where the certificate store being managed resides.  
**SeparateUploadFilePath** (Applicable for Linux managed servers only) – Only used when UseSeparateUploadFilePath is set to "Y". Set this to the path you wish to use as the location to upload and later remove PEM/PKCS12 certificate store data before being moved to the final destination.  
**UseNegotiateAuth** (Applicable for Windows managed servers only) – Y/N - Determines if WinRM should use Negotiate (Y) when connecting to the remote server.  
**UseSCP** (Optional, Applicable for Linux managed servers only) - Y/N - Detemines if SCP (Y) or SFTP (N) should be used in uploading certificate files during Management-Add jobs.  
**DefaultLinuxPermissionsOnStoreCreation** (Applicable for Linux managed servers only) - Optional.  Value must be 3 digits all between 0-7.  The Linux file permissions that will be set on a new certificate store created via a Management Create job.  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  If "Linux File Permissions on Store Creation" and DefaultLinuxPermissionsOnStoreCreation are not set, a default permission of 600 will be used.

***

### License
[Apache](https://apache.org/licenses/LICENSE-2.0)

