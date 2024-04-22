
# Remote File

The Remote File Orchestrator allows for the remote management of file-based certificate stores. Discovery, Inventory, and Management functions are supported. The orchestrator performs operations by first converting the certificate store into a BouncyCastle PKCS12Store.

#### Integration status: Production - Ready for use in production environments.

## About the Keyfactor Universal Orchestrator Extension

This repository contains a Universal Orchestrator Extension which is a plugin to the Keyfactor Universal Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Universal Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing Extensions, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific Extension see below in this readme.

The Universal Orchestrator is the successor to the Windows Orchestrator. This Orchestrator Extension plugin only works with the Universal Orchestrator and does not work with the Windows Orchestrator.

## Support for Remote File

Remote File 

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

---


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
  

It is not necessary to use a PAM Provider for all of the secrets available above. If a PAM Provider should not be used, simply enter in the actual value to be used, as normal.

If a PAM Provider will be used for one of the fields above, start by referencing the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam). The GitHub repo for the PAM Provider to be used contains important information such as the format of the `json` needed. What follows is an example but does not reflect the `json` values for all PAM Providers as they have different "instance" and "initialization" parameter names and values.

<details><summary>General PAM Provider Configuration</summary>
<p>



### Example PAM Provider Setup

To use a PAM Provider to resolve a field, in this example the __Server Password__ will be resolved by the `Hashicorp-Vault` provider, first install the PAM Provider extension from the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam) on the Universal Orchestrator.

Next, complete configuration of the PAM Provider on the UO by editing the `manifest.json` of the __PAM Provider__ (e.g. located at extensions/Hashicorp-Vault/manifest.json). The "initialization" parameters need to be entered here:

~~~ json
  "Keyfactor:PAMProviders:Hashicorp-Vault:InitializationInfo": {
    "Host": "http://127.0.0.1:8200",
    "Path": "v1/secret/data",
    "Token": "xxxxxx"
  }
~~~

After these values are entered, the Orchestrator needs to be restarted to pick up the configuration. Now the PAM Provider can be used on other Orchestrator Extensions.

### Use the PAM Provider
With the PAM Provider configured as an extenion on the UO, a `json` object can be passed instead of an actual value to resolve the field with a PAM Provider. Consult the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam) for the specific format of the `json` object.

To have the __Server Password__ field resolved by the `Hashicorp-Vault` provider, the corresponding `json` object from the `Hashicorp-Vault` extension needs to be copied and filed in with the correct information:

~~~ json
{"Secret":"my-kv-secret","Key":"myServerPassword"}
~~~

This text would be entered in as the value for the __Server Password__, instead of entering in the actual password. The Orchestrator will attempt to use the PAM Provider to retrieve the __Server Password__. If PAM should not be used, just directly enter in the value for the field.
</p>
</details> 




---


﻿<!-- add integration specific information below -->
## Overview
The Remote File Orchestrator Extension is a multi-purpose integration that can remotely manage a variety of file-based certificate stores and can easily be extended to manage others.  The certificate store types that can be managed in the current version are:
- Java Keystores of type JKS
- PKCS12 files, including, but not limited to, Java keystores of type PKCS12
- PEM formatted files
- DER formatted files
- IBM Key Database files (KDB)
- Oracle Wallet Pkcs12 files

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

## Prerequisites and Security Considerations

<details>
<summary><b>Certificate stores hosted on Linux servers:</b></summary>
1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y" (See "Config File Setup" later in this README for more information on setting up the config.json file). The full list of these commands below:

|Shell Command|Used For|
|---|---|
|echo|Used to append a newline and terminate all commands sent.|
|find|Used by Discovery jobs to locate potential certificate stores on the file system.|
|cp|Used by Inventory and Management Add/Remove jobs to copy the certificate store file to a temporary file (only when an alternate download folder has been configured).|
|chown|Used by the Inventory and Management Add/Remove jobs to set the permissions on the temporary file (only when an alternate download folder has been configured).|
|tee|Used by Management Add/Remove jobs to copy the temporary uploaded certificate file to the certificate store file (only when an alternate upload folder has been configured).|
|rm|Used by Inventory and Management Add/Remove jobs to remove temporary files (only when an alternate upload/download folder has been configured).|
|install|Used by the Management Create Store job when initializing a certificate store file.|
|orapki|Oracle Wallet CLI utility used by Inventory and Management Add/Remove jobs to manipulate an Oracle Wallet certificate store.  Used for the RFORA store type only.|
|gskcapicmd|IBM Key Database CLI utility used by Inventory and Management Add/Remove jobs to manipulate an IBM Key Database certificate store.  Used for the RFKDB store type only.|

2. The Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer (See "Config File Setup" later in this README regarding the config.json file).

3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension (see "Creating Certificate Stores" later in this README, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  
</details>  

<details>  
<summary><b>Certificate stores hosted on Windows servers:</b></summary>
1. Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores When creating a new certificate store in Keyfactor Command (See "Creating Certificate Stores" later in this README).

Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.
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
</details>  


&nbsp;
## Configuration File Setup 

The Remote File Orchestrator Extension uses a JSON configuration file.  It is located in the {Keyfactor Orchestrator Installation Folder}\Extensions\RemoteFile.  None of the values are required, and a description of each follows below:  
{  
   "UseSudo": "N",  
   "DefaultSudoImpersonatedUser": "",  
   "CreateStoreIfMissing": "N",  
   "UseNegotiate": "N",  
   "SeparateUploadFilePath": "",  
   "FileTransferProtocol":  "SCP",  
   "DefaultLinuxPermissionsOnStoreCreation": "600",  
   "DefaultOwnerOnStoreCreation": ""  
}  

<details>
<summary><b>UseSudo</b> (Applicable for Linux hosted certificate stores only)</summary>

* Determines whether to prefix certain Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
* Allowed values - Y/N  
* Default value - N  

</details>  

<details>
<summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

* Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the Certificate Store Types section.
* Allowed values - Any valid user id that the destination Linux server will recognize
* Default value - blank (root will be used)

</details>

<details>  
<summary><b>CreateStoreOnAddIfMissing</b></summary>

* Determines, during a Management-Add job, if a certificate store should be created if it does not already exist.  If set to "N", and the store referenced in the Management-Add job is not found, the job will return an error with a message stating that the store does not exist.  If set to "Y", the store will be created and the certificate added to the certificate store.
* Allowed values - Y/N  
* Default value - N  

</details>  

<details>  
<summary><b>UseNegotiateAuth</b> (Applicable for Windows hosted certificate stores only)</summary>

* Determines if WinRM should use Negotiate (Y) when connecting to the remote server.
* Allowed values - Y/N  
* Default value - N  

</details>  

<details>  
<summary><b>SeparateUploadFilePath</b>(Applicable for Linux hosted certificate stores only)</summary>

* Set this to the path you wish to use as the location on the orchestrated server to upload/download and later remove temporary work files when processing jobs.  If set to "" or not provided, the location of the certificate store itself will be used.  File transfer is performed using the SCP or SFTP protocols (see the File TransferProtocol setting).
* Allowed values - Any valid, existing Linux path configured to allow SCP/SFTP file upload/download tranfers.
* Default value - blank (actual store path will be used)

</details>

<details>  
<summary><b>FileTransferProtocol</b> (Applicable for Linux hosted certificate stores only)</summary>

* Determines the protocol to use when uploading/downloading files while processing a job.
* Allowed values - SCP, SFTP or Both.  If "Both" is entered, SCP will be attempted first, and if that does not work, SFTP will be tried.
* Default value - SCP. 

</details>

<details>  
<summary><b>DefaultLinuxPermissionsOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

* The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store (See the "Certificatee Store Types Supported" section later in this README).
* Allowed values - Any 3 digit value from 000-777.
* Default Value - 600.  

</details>

<details>  
<summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux managed servers only)</summary>

* When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store (See the "Certificatee Store Types Supported" section later in this README) can override this value for a specific store.
* Allowed values - Any valid user id that the destination Linux server will recognize
* Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

</details>

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
- **Uses PowerShell** - Unchecked
- **Requires Store Password** - Checked.  NOTE: This does not require that a certificate store have a password, but merely ensures that a user who creates a Keyfactor Command Certificate Store MUST click the Store Password button and either enter a password or check No Password.  Certificate stores with no passwords are still possible for certain certificate store types when checking this option.
- **Supports Entry Password** - Unchecked.  

*Advanced Tab:*  
- **Store Path Type** - Freeform
- **Supports Custom Alias** - See specific certificate store type instructions below.
- **Private Key Handling** - See specific certificate store type instructions below
- **PFX Password Style** - Default  

*Custom Fields Tab:*
- **Name:** LinuxFilePermissionsOnStoreCreation, **Display Name:** Linux File Permissions on Store Creation, **Type:** String, **Default Value:** none.  This custom field is **not required**.  If not present, value reverts back to the DefaultLinuxPermissionsOnStoreCreation setting in config.json (see Configuration File Setup section above).  This value, applicable to certificate stores hosted on Linux orchestrated servers only, must be 3 digits all between 0-7.  This represents the Linux file permissions that will be set for this certificate store if created via a Management Create job or a Management Add job where the config.json option CreateStoreOnAddIsMissing is set to "Y".  
- **Name:** LinuxFileOwnerOnStoreCreation, **Display Name:** Linux File Owner on Store Creation, **Type:** String, **Default Value:** none.  This custom field is **not required**.  If not present, value reverts back to the DefaultOwnerOnStoreCreation setting in config.json (see Configuration File Setup section above).  This value, applicable to certificate stores hosted on Linux orchestrated servers only, represents the alternate Linux file owner/group that will be set for this certificate store if created via a Management Create job or a Management Add job where the config.json option CreateStoreOnAddIsMissing is set to "Y".  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please confirm that the user name associated with this Keyfactor certificate store has valid permissions to chown the certificate file to this owner.
- **Name:** SudoImpersonatedUser, **Display Name:** Sudo Impersonated User Id, **Type:** String, **Default Value:** none.  This custom field is **not required**.  If not present, value reverts back to the DefaultSudoImpersonatedUser setting in config.json (see Configuration File Setup section above).  Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is empty, and nothing is set for DefaultSudoImpersonatedUser in your config.json, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see Configuration File Setup section above) as well as permissions to execute the commands listed in the "Security Considerations" section above.

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
- **Name:** IgnorePrivateKeyOnInventory, **Display Name:** Ignore Private Key On Inventory, **Type:** Bool, **Default Value:** false.   This custom field is **not required**. Default value if not present is 'false'.  If 'true', inventory for this certificate store will be performed without accessing the certificate's private key or the store password.  This will functionally make the store INVENTORY ONLY, as all certificates will be returned with "Private Key Entry" = false.  Also, no certificate chain relationships will be maintained, and all certificates will be considered separate entries (basically a trust store).  This may be useful in situations where the client does not know the store password at inventory run time, but would still like the certificates to be imported into Keyfactor Command.  Once the correct store password is entered for the store, the client may de-select this option (change the value to False), schedule an inventory job, and then the appropriate private key entry and chain information should be properly stored in the Keyfactor Command location, allowing for renewal/removal of the certificate at a later time. 

Entry Parameters Tab:
- no additional entry parameters

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
&nbsp;  
**************************************
**RFKDB Certificate Store Type**
**************************************

The RFKDB store type can be used to manage IBM Key Database Files (KDB) files.  The IBM utility, GSKCAPICMD, is used to read and write certificates from and to the target store and is therefore required to be installed on the server where each KDB certificate store being managed resides, and its location MUST be in the system $Path.

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
&nbsp;  
**************************************
**RFORA Certificate Store Type**
**************************************

The RFORA store type can be used to manage Pkcs2 Oracle Wallets.  Please note that while this should work for Pkcs12 Oracle Wallets installed on both Windows and Linux servers, this has only been tested on wallets installed on Windows.  Please note, when entering the Store Path for an Oracle Wallet in Keyfactor Command, make sure to INCLUDE the eWallet.p12 file name that by convention is the name of the Pkcs12 wallet file that gets created.

Use cases supported:
1. One-to-many trust entries - A single certificate without a private key in a certificate store.  Each certificate identified with a custom alias or certificate thumbprint.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias or certificate thumbprint.
3. A mix of trust and key entries.

**Specific Certificate Store Type Values**  
*Basic Tab:*
- **Short Name** – Required. Suggested value - **RFORA**.  If you choose to use a different value you must make the corresponding modification to the manifest.json file (see "Remote File Orchestrator Extension Installation", step 6 above).

*Advanced Tab:*
- **Supports Custom Alias** - Required.
- **Private Key Handling** - Optional.  

*Custom Fields Tab:*  
- **Name:** WorkFolder, **Display Name:** Work Folder, **Type:** String, **Default Value:** empty.   This custom field is **required**. This required field should contain the path on the managed server where temporary work files can be created during Inventory and Management jobs.  These files will be removed at the end of each job  Please make sure that user id you have assigned to this certificate store will have access to create, modify, and delete files from this folder. 

Entry Parameters Tab:
- no additional entry parameters  

&nbsp;  
&nbsp;  
## Creating Certificate Stores and Scheduling Discovery Jobs

Please refer to the Keyfactor Command Reference Guide for information on creating certificate stores and scheduling Discovery jobs in Keyfactor Command.  However, there are a few fields that are important to highlight here - Client Machine, Store Path (Creating Certificate Stores), and Directories to search (Discovery jobs) and Extensions (Discovery jobs).  For Linux orchestrated servers, "Client Machine" should be the DNS or IP address of the remote orchestrated server while "Store Path" is the full path and file name of the file based store, beginning with a forward slash (/).  For Windows orchestrated servers, "Client Machine" should be of the format {protocol}://{dns-or-ip}:{port} where {protocol} is either http or https, {dns-or-ip} is the DNS or IP address of the remote orchestrated server, and {port} is the port where WinRM is listening, by convention usually 5985 for http and 5986 for https.  Alternately, entering the keyword "localhost" for "Client Machine" will point to the server where the orchestrator service is installed and WinRM WILL NOT be required.  "Store Path" is the full path and file name of the file based store, beginning with a drive letter (i.e. c:\).  For example valid values for Client Machine and Store Path for Linux and Windows managed servers may look something like:  

Linux: Client Machine - 127.0.0.1 or MyLinuxServerName; Store Path - /home/folder/path/storename.ext  
Windows: Client Machine - http<span>s://My.Server.Domain:59</span>86; Store Path - c:\folder\path\storename.ext  

Credentials **must** be entered: a user id and either a password or valid PAM key if the password is stored in a KF Command configured PAM integration.  Alternatively, this password can be an SSH private key if connecting to a Linux server using SSH private key authentication.

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
4. Create a new class (with namespace of Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}) in the new folder that will implement ICertificateStoreSerializer.  By convention, {StoreTypeName}CertificateSerializer would be a good choice for the class name.  This interface requires you to implement three methods:
    - DesrializeRemoteCertificateStore - This method takes in a byte array containing the contents of file based store you are managing.  The developer will need to convert that to an Org.BouncyCastle.Pkcs.Pkcs12Store class and return it. 
    - SerializeRemoteCertificateStore - This method takes in an Org.BouncyCastle.Pkcs.Pkcs12Store and converts it to a collection of custom file representations.  
    - GetPrivateKeyPath - This method returns the location of the external private key file for single certificate stores.  Currently this is only used for RFPEM, and all other implementations return NULL for this method.  If this is not applicable to your implementation just return a NULL value for this method. 
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


