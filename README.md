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

The Remote File Orchestrator Extension is a multi-purpose integration that can remotely manage a variety of file-based certificate stores and can easily be extended to manage others.  The certificate store types that can be managed in the current version are: 

* RFJKS - Java Keystores of types JKS or PKCS12
* RFPkcs12 - Certificate stores that follow the PKCS#12 standard
* RFPEM - Files in PEM format
* RFDER - Files in binary DER format
* RFORA - Pkcs#12 formatted Oracle Wallets
* RFKDB - IBM Key Database files

The Keyfactor Univeral Orchestrator (UO) and RemoteFile Extension can be installed on either Windows or Linux operating systems as well as manage certificates residing on servers of both operating systems. A UO service managing certificates on remote servers is considered to be acting as an Orchestrator, while a UO service managing local certificates on the same server running the service is considered an Agent.  When acting as an Orchestrator, connectivity from the orchestrator server hosting the RemoteFile extension to the orchestrated server hosting the certificate store(s) being managed is achieved via either an SSH (for Linux and possibly Windows orchestrated servers) or WinRM (for Windows orchestrated servers) connection.  When acting as an agent, SSH/WinRM may still be used, OR the certificate store can be configured to bypass these and instead directly access the orchestrator server's file system.

![](images/orchestrator-agent.png)  

Please review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) sections for more information on proper configuration and setup for these different architectures.  The supported configurations of Universal Orchestrator hosts and managed orchestrated servers are detailed below:

| | UO Installed on Windows | UO Installed on Linux |
|-----|-----|------|
|Orchestrated Server hosting certificate store(s) on remote Windows server|WinRM connection | SSH connection |
|Orchestrated Server hosting certificate store(s) on remote Linux server| SSH connection | SSH connection |
|Certificate store(s) on same server as orchestrator service (Agent)| WinRM connection or local file system | SSH connection or local file system |

## Compatibility

This integration is compatible with Keyfactor Universal Orchestrator version 10.4 and later.

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

    3. SSH Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  When using a password, the connection is attempted using SSH Password Authentication.  If that fails, Keyboard Interactive Authentication is automatically attempted.  One or both of these must be enabled on the Linux box being managed.  If private key authentication is desired, copy and paste the full SSH private key into the Password textbox (or pointer to the private key if using a PAM provider).  Please note that SSH Private Key Authentication is not available when running locally as an agent.  The following private key formats are supported: 
    - PKCS#1 (BEGIN RSA PRIVATE KEY) 
    - PKCS#8 (BEGIN PRIVATE KEY)
    - ECDSA OPENSSH (BEGIN OPENSSH PRIVATE KEY)   

    Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
    </details>  

    <details>  
    <summary><b>Certificate stores hosted on Windows servers:</b></summary>
    1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

    </details>

    Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

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

TODO Installation is an optional section - if you don't need it, feel free to remove it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info


</details>

<details><summary>RFPkcs12</summary>

TODO Installation is an optional section - if you don't need it, feel free to remove it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info


</details>

<details><summary>RFDER</summary>

TODO Installation is an optional section - if you don't need it, feel free to remove it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info


</details>

<details><summary>RFKDB</summary>

TODO Installation is an optional section - if you don't need it, feel free to remove it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info


</details>

<details><summary>RFORA</summary>

TODO Installation is an optional section - if you don't need it, feel free to remove it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info


</details>


## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).