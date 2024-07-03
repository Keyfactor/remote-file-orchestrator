<h1 align="center" style="border-bottom: none">
    Remote File Universal Orchestrator Extension
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/remote-file-orchestrator/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/remote-file-orchestrator?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/remote-file-orchestrator?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/remote-file-orchestrator/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a>
  ·
  <a href="#installation">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=orchestrator">
    <b>Related Integrations</b>
  </a>
</p>


## Overview

The Remote File Universal Orchestrator extension is a versatile integration that facilitates the remote management of various file-based certificate stores. This extension is capable of managing multiple store types, providing comprehensive support for certificates in different formats. The supported certificate store types include:

- **RFPkcs12**: Manages any PKCS#12 compliant file format, including Java keystores of type PKCS12. It supports one-to-many trust entries, one-to-many key entries, and a mix of trust and key entries.
- **RFJKS**: Manages Java keystores of type JKS. Unlike RFPkcs12, this type strictly supports Java keystores of type JKS and not PKCS12.
- **RFPEM**: Manages PEM encoded files. This store type supports trust stores, single certificate stores with a private key in the file, single certificate stores with a certificate chain and private key in the file, and configurations with external private key files.
- **RFDER**: Manages DER encoded files, supporting single certificate stores with an external private key file and without a private key.
- **RFKDB**: Manages IBM Key Database Files (KDB). Requires the IBM utility, GSKCAPICMD, to be installed on the server where the KDB file resides. Supports one-to-many trust entries, key entries, and a mix of both.
- **RFORA**: Manages Pkcs12 Oracle Wallets, primarily tested on wallets installed on Windows. It supports one-to-many trust entries, key entries, and a mix of both.

Each of these certificate store types represents a unique format or structure for storing cryptographic certificates and keys. The main differences between them lie in the specific file format they manage and the use cases they support. For instance, RFPkcs12 can manage both trust and key entries, while RFPEM is highly versatile with its support for both internal and external private key files. In contrast, RFDER is limited to single certificate stores with optional external private keys.

The Keyfactor Universal Orchestrator (UO) and Remote File Extension can be installed on Windows or Linux operating systems, and manage certificates on servers of both types. When running on a remote server, the extension acts as an Orchestrator, connecting via SSH (for Linux) or WinRM (for Windows). When running locally on the same server, it acts as an Agent, potentially bypassing SSH/WinRM and directly accessing the file system.

By integrating closely with Keyfactor Command, the Remote File Universal Orchestrator extension simplifies and automates the management of certificates across diverse environments and certificate store types.

## Compatibility

This integration is compatible with Keyfactor Universal Orchestrator version 10.1 and later.

## Support
The Remote File Universal Orchestrator extension is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 
 
> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Installation
Before installing the Remote File Universal Orchestrator extension, it's recommended to install [kfutil](https://github.com/Keyfactor/kfutil). Kfutil is a command-line tool that simplifies the process of creating store types, installing extensions, and instantiating certificate stores in Keyfactor Command.

The Remote File Universal Orchestrator extension implements 6 Certificate Store Types. Depending on your use case, you may elect to install one, or all of these Certificate Store Types. An overview for each type is linked below:
* [RFJKS](docs/rfjks.md)
* [RFPEM](docs/rfpem.md)
* [RFPkcs12](docs/rfpkcs12.md)
* [RFDER](docs/rfder.md)
* [RFKDB](docs/rfkdb.md)
* [RFORA](docs/rfora.md)

<details><summary>RFJKS</summary>


1. Follow the [requirements section](docs/rfjks.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFJKS
        kfutil store-types create RFJKS
        ```

    * **Manually**:
        * [RFJKS](docs/rfjks.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFJKS](docs/rfjks.md#certificate-store-configuration)


</details>

<details><summary>RFPEM</summary>


1. Follow the [requirements section](docs/rfpem.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;

    ### Configuration File Setup

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

    * Determines whether to prefix Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
    * Allowed values - Y/N  
    * Default value - N  

    </details>  

    <details>
    <summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

    * Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the [Creating Certificate Store Types](#creating-certificate-store-types) section.
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
    <summary><b>SeparateUploadFilePath</b> (Applicable for Linux hosted certificate stores only)</summary>

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

    * The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any 3 digit value from 000-777.
    * Default Value - 600.  

    </details>

    <details>  
    <summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

    * When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store can override this value for a specific store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any valid user id that the destination Linux server will recognize
    * Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

    </details>

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFPEM
        kfutil store-types create RFPEM
        ```

    * **Manually**:
        * [RFPEM](docs/rfpem.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFPEM](docs/rfpem.md#certificate-store-configuration)


</details>

<details><summary>RFPkcs12</summary>


1. Follow the [requirements section](docs/rfpkcs12.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;

    ### Configuration File Setup

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

    * Determines whether to prefix Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
    * Allowed values - Y/N  
    * Default value - N  

    </details>  

    <details>
    <summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

    * Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the [Creating Certificate Store Types](#creating-certificate-store-types) section.
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
    <summary><b>SeparateUploadFilePath</b> (Applicable for Linux hosted certificate stores only)</summary>

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

    * The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any 3 digit value from 000-777.
    * Default Value - 600.  

    </details>

    <details>  
    <summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

    * When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store can override this value for a specific store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any valid user id that the destination Linux server will recognize
    * Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

    </details>

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFPkcs12
        kfutil store-types create RFPkcs12
        ```

    * **Manually**:
        * [RFPkcs12](docs/rfpkcs12.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFPkcs12](docs/rfpkcs12.md#certificate-store-configuration)


</details>

<details><summary>RFDER</summary>


1. Follow the [requirements section](docs/rfder.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;

    ### Configuration File Setup

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

    * Determines whether to prefix Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
    * Allowed values - Y/N  
    * Default value - N  

    </details>  

    <details>
    <summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

    * Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the [Creating Certificate Store Types](#creating-certificate-store-types) section.
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
    <summary><b>SeparateUploadFilePath</b> (Applicable for Linux hosted certificate stores only)</summary>

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

    * The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any 3 digit value from 000-777.
    * Default Value - 600.  

    </details>

    <details>  
    <summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

    * When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store can override this value for a specific store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any valid user id that the destination Linux server will recognize
    * Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

    </details>

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFDER
        kfutil store-types create RFDER
        ```

    * **Manually**:
        * [RFDER](docs/rfder.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFDER](docs/rfder.md#certificate-store-configuration)


</details>

<details><summary>RFKDB</summary>


1. Follow the [requirements section](docs/rfkdb.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;

    ### Configuration File Setup

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

    * Determines whether to prefix Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
    * Allowed values - Y/N  
    * Default value - N  

    </details>  

    <details>
    <summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

    * Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the [Creating Certificate Store Types](#creating-certificate-store-types) section.
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
    <summary><b>SeparateUploadFilePath</b> (Applicable for Linux hosted certificate stores only)</summary>

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

    * The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any 3 digit value from 000-777.
    * Default Value - 600.  

    </details>

    <details>  
    <summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

    * When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store can override this value for a specific store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any valid user id that the destination Linux server will recognize
    * Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

    </details>

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFKDB
        kfutil store-types create RFKDB
        ```

    * **Manually**:
        * [RFKDB](docs/rfkdb.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFKDB](docs/rfkdb.md#certificate-store-configuration)


</details>

<details><summary>RFORA</summary>


1. Follow the [requirements section](docs/rfora.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### Prerequisites and Security Considerations

    <details>
    <summary><b>Certificate stores hosted on Linux servers:</b></summary>

    1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

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

    2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

    3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

      
    &nbsp;

    ### Remote File Orchestrator Extension Installation
    1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
    2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
    3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
    4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
    5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
    6. Copy the contents of the download installation zip file to the folder created in step 4.
    7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
    8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
    9. Start the Keyfactor Universal Orchestrator Service.

    </details>  

    &nbsp;

    ### Configuration File Setup

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

    * Determines whether to prefix Linux command with "sudo". This can be very helpful in ensuring that the user id running commands over an ssh connection uses "least permissions necessary" to process each task. Setting this value to "Y" will prefix all Linux commands with "sudo" with the expectation that the command being executed on the orchestrated Linux server will look in the sudoers file to determine whether the logged in ID has elevated permissions for that specific command. Setting this value to "N" will result in "sudo" not being added to Linux commands.  
    * Allowed values - Y/N  
    * Default value - N  

    </details>  

    <details>
    <summary><b>DefaultSudoImpersonatedUser</b> (Applicable for Linux hosted certificate stores only)</summary>

    * Used in conjunction with UseSudo="Y", this optional setting can be used to set an alternate user id you wish to impersonate with sudo.  If this option does not exist or is set to an empty string, the default user of "root" will be used.  Any user id used here must have permissions to SCP/SFTP files to/from each certificate store location OR the SeparateUploadFilePath (see later in this section) as well as permissions to execute the commands listed in the "Prerequisites and Security Considerations" section above.  This value will be used for all certificate stores managed by this orchestrator extension implementation UNLESS overriden by the SudoImpersonatedUser certificate store type custom field setting described later in the [Creating Certificate Store Types](#creating-certificate-store-types) section.
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
    <summary><b>SeparateUploadFilePath</b> (Applicable for Linux hosted certificate stores only)</summary>

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

    * The Linux file permissions that will be set on a new certificate store created via a Management Create job or a Management Add job where CreateStoreOnAddIsMissing is set to "Y".  This value will be used for all certificate stores managed by this orchestrator instance unless overridden by the optional "Linux File Permissions on Store Creation" custom parameter setting on a specific certificate store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any 3 digit value from 000-777.
    * Default Value - 600.  

    </details>

    <details>  
    <summary><b>DefaultOwnerOnStoreCreation</b> (Applicable for Linux hosted certificate stores only)</summary>

    * When a Management job is run to remotely create the physical certificate store on a remote server, by default the file owner and group will be set to the user name associated with the Keyfactor certificate store.  Setting DefaultOwnerOnStoreCreation to an alternative valid Linux user name will set that as the owner/group instead.  If the group and owner need to be different values, use a ":" as a delimitter between the owner and group values, such as ownerId:groupId.  Please make sure that the user associated with the certificate store will have valid permissions to chown the certificate store file to this alernative owner.  The optional "Linux File Owner on Store Creation" custom parameter setting for a specific certificate store can override this value for a specific store.  See the [Creating Certificate Store Types](#creating-certificate-store-types) section for more information on creating RemoteFile certificate store types.
    * Allowed values - Any valid user id that the destination Linux server will recognize
    * Default Value - blank (the ID associated with the Keyfactor certificate store will be used).  

    </details>

    &nbsp;



    </details>

2. Create Certificate Store Types for the Remote File Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # RFORA
        kfutil store-types create RFORA
        ```

    * **Manually**:
        * [RFORA](docs/rfora.md#certificate-store-type-configuration)

3. Install the Remote File Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e remote-file-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Remote File Universal Orchestrator extension](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [RFORA](docs/rfora.md#certificate-store-configuration)


</details>


## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).