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

Please refer to the READMEs for each supported store type for more information on proper configuration and setup for these different architectures.  The supported configurations of Universal Orchestrator hosts and managed orchestrated servers are detailed below:

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

Before installing the Remote File Universal Orchestrator extension, we recommend that you install [kfutil](https://github.com/Keyfactor/kfutil). Kfutil is a command-line tool that simplifies the process of creating store types, installing extensions, and instantiating certificate stores in Keyfactor Command.

1. **Create Certificate Store Types in Keyfactor Command**  
The Remote File Universal Orchestrator extension implements 6 Certificate Store Types. Depending on your use case, you may elect to install one, or all of these Certificate Store Types.

    <details><summary>RFJKS</summary>


    > More information on the RFJKS Certificate Store Type can be found [here](docs/rfjks.md).

    * **Create RFJKS using kfutil**:

        ```shell
        # RFJKS
        kfutil store-types create RFJKS
        ```

    * **Create RFJKS manually in the Command UI**:
        
        Refer to the [RFJKS](docs/rfjks.md#certificate-store-type-configuration) creation docs.
    </details>

    <details><summary>RFPEM</summary>


    > More information on the RFPEM Certificate Store Type can be found [here](docs/rfpem.md).

    * **Create RFPEM using kfutil**:

        ```shell
        # RFPEM
        kfutil store-types create RFPEM
        ```

    * **Create RFPEM manually in the Command UI**:
        
        Refer to the [RFPEM](docs/rfpem.md#certificate-store-type-configuration) creation docs.
    </details>

    <details><summary>RFPkcs12</summary>


    > More information on the RFPkcs12 Certificate Store Type can be found [here](docs/rfpkcs12.md).

    * **Create RFPkcs12 using kfutil**:

        ```shell
        # RFPkcs12
        kfutil store-types create RFPkcs12
        ```

    * **Create RFPkcs12 manually in the Command UI**:
        
        Refer to the [RFPkcs12](docs/rfpkcs12.md#certificate-store-type-configuration) creation docs.
    </details>

    <details><summary>RFDER</summary>


    > More information on the RFDER Certificate Store Type can be found [here](docs/rfder.md).

    * **Create RFDER using kfutil**:

        ```shell
        # RFDER
        kfutil store-types create RFDER
        ```

    * **Create RFDER manually in the Command UI**:
        
        Refer to the [RFDER](docs/rfder.md#certificate-store-type-configuration) creation docs.
    </details>

    <details><summary>RFKDB</summary>


    > More information on the RFKDB Certificate Store Type can be found [here](docs/rfkdb.md).

    * **Create RFKDB using kfutil**:

        ```shell
        # RFKDB
        kfutil store-types create RFKDB
        ```

    * **Create RFKDB manually in the Command UI**:
        
        Refer to the [RFKDB](docs/rfkdb.md#certificate-store-type-configuration) creation docs.
    </details>

    <details><summary>RFORA</summary>


    > More information on the RFORA Certificate Store Type can be found [here](docs/rfora.md).

    * **Create RFORA using kfutil**:

        ```shell
        # RFORA
        kfutil store-types create RFORA
        ```

    * **Create RFORA manually in the Command UI**:
        
        Refer to the [RFORA](docs/rfora.md#certificate-store-type-configuration) creation docs.
    </details>

2. **Download the latest Remote File Universal Orchestrator extension from GitHub.** 

    On the [Remote File Universal Orchestrator extension GitHub version page](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest), click the `remote-file-orchestrator` asset to download the zip archive. Unzip the archive containing extension assemblies to a known location.

3. **Locate the Universal Orchestrator extensions directory.**

    * **Default on Windows** - `C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions`
    * **Default on Linux** - `/opt/keyfactor/orchestrator/extensions`
    
4. **Create a new directory for the Remote File Universal Orchestrator extension inside the extensions directory.**
        
    Create a new directory called `remote-file-orchestrator`.
    > The directory name does not need to match any names used elsewhere; it just has to be unique within the extensions directory.

5. **Copy the contents of the downloaded and unzipped assemblies from __step 2__ to the `remote-file-orchestrator` directory.**

6. **Restart the Universal Orchestrator service.**

    Refer to [Starting/Restarting the Universal Orchestrator service](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/StarttheService.htm).



> The above installation steps can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions).

## Configuration and Usage

The Remote File Universal Orchestrator extension implements 6 Certificate Store Types, each of which implements different functionality. Refer to the individual instructions below for each Certificate Store Type that you deemed necessary for your use case from the installation section.

<details><summary>RFJKS</summary>

1. Refer to the [requirements section](docs/rfjks.md#requirements) to ensure all prerequisites are met before using the RFJKS Certificate Store Type.
2. Create new [RFJKS](docs/rfjks.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>

<details><summary>RFPEM</summary>

1. Refer to the [requirements section](docs/rfpem.md#requirements) to ensure all prerequisites are met before using the RFPEM Certificate Store Type.
2. Create new [RFPEM](docs/rfpem.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>

<details><summary>RFPkcs12</summary>

1. Refer to the [requirements section](docs/rfpkcs12.md#requirements) to ensure all prerequisites are met before using the RFPkcs12 Certificate Store Type.
2. Create new [RFPkcs12](docs/rfpkcs12.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>

<details><summary>RFDER</summary>

1. Refer to the [requirements section](docs/rfder.md#requirements) to ensure all prerequisites are met before using the RFDER Certificate Store Type.
2. Create new [RFDER](docs/rfder.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>

<details><summary>RFKDB</summary>

1. Refer to the [requirements section](docs/rfkdb.md#requirements) to ensure all prerequisites are met before using the RFKDB Certificate Store Type.
2. Create new [RFKDB](docs/rfkdb.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>

<details><summary>RFORA</summary>

1. Refer to the [requirements section](docs/rfora.md#requirements) to ensure all prerequisites are met before using the RFORA Certificate Store Type.
2. Create new [RFORA](docs/rfora.md#certificate-store-configuration) Certificate Stores in Keyfactor Command.
</details>


## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).