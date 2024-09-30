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

TODO this section is required

The Remote File Universal Orchestrator extension implements 6 Certificate Store Types. Depending on your use case, you may elect to use one, or all of these Certificate Store Types. Descriptions of each are provided below.

<details><summary>RFJKS</summary>

### RFJKS
The RFJKS store type can be used to manage java keystores of types JKS or PKCS12.  If creating a new java keystore and adding a certificate all via Keyfactor Command, the created java keystore will be of type PKCS12, as java keystores of type JKS have been deprecated as of JDK 9.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.
</details>

<details><summary>RFPEM</summary>

### RFPEM
The RFPEM store type can be used to manage PEM encoded files.

Use cases supported:
1. Trust stores - A file with one-to-many certificates (no private keys, no certificate chains).
2. Single certificate stores with private key in the file.
3. Single certificate stores with certificate chain and private key in the file.
4. Single certificate stores with private key in an external file.
5. Single certificate stores with certificate chain in the file and private key in an external file

NOTE: PEM stores may only have one private key (internal or external) associated with the store, as only one certificate/chain/private key combination can be stored in a PEM store supported by RFPEM.
</details>

<details><summary>RFPkcs12</summary>

### RFPkcs12
The RFPkcs12 store type can be used to manage any PKCS#12 compliant file format INCLUDING java keystores of type PKCS12.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.
</details>

<details><summary>RFDER</summary>

### RFDER
The RFORA store type can be used to manage Pkcs12 Oracle Wallets.  Please note that while this should work for Pkcs12 Oracle Wallets installed on both Windows and Linux servers, this has only been tested on wallets installed on Windows.  Please note, when entering the Store Path for an Oracle Wallet in Keyfactor Command, make sure to INCLUDE the eWallet.p12 file name that by convention is the name of the Pkcs12 wallet file that gets created.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.
</details>

<details><summary>RFKDB</summary>

### RFKDB
The RFKDB store type can be used to manage IBM Key Database Files (KDB) files.  The IBM utility, GSKCAPICMD, is used to read and write certificates from and to the target store and is therefore required to be installed on the server where each KDB certificate store being managed resides, and its location MUST be in the system $Path.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.
</details>

<details><summary>RFORA</summary>

### RFORA
The RFORA store type can be used to manage Pkcs12 Oracle Wallets.  Please note that while this should work for Pkcs12 Oracle Wallets installed on both Windows and Linux servers, this has only been tested on wallets installed on Windows.  Please note, when entering the Store Path for an Oracle Wallet in Keyfactor Command, make sure to INCLUDE the eWallet.p12 file name that by convention is the name of the Pkcs12 wallet file that gets created.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.
</details>


## Compatibility

This integration is compatible with Keyfactor Universal Orchestrator version 10.4 and later.

## Support
The Remote File Universal Orchestrator extension is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 
 
> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Requirements & Prerequisites

Before installing the Remote File Universal Orchestrator extension, we recommend that you install [kfutil](https://github.com/Keyfactor/kfutil). Kfutil is a command-line tool that simplifies the process of creating store types, installing extensions, and instantiating certificate stores in Keyfactor Command.



The Remote File Universal Orchestrator extension implements 6 Certificate Store Types. Depending on your use case, you may elect to install one, or all of these Certificate Store Types.

<details><summary>RFJKS</summary>


TODO Requirements is a required section
</details>

<details><summary>RFPEM</summary>


TODO Requirements is a required section
</details>

<details><summary>RFPkcs12</summary>


TODO Requirements is a required section
</details>

<details><summary>RFDER</summary>


TODO Requirements is a required section
</details>

<details><summary>RFKDB</summary>


TODO Requirements is a required section
</details>

<details><summary>RFORA</summary>


TODO Requirements is a required section
</details>


### Create Certificate Store Types
<details><summary>RFJKS</summary>


* **Create RFJKS using kfutil**:

    ```shell
    # RFJKS
    kfutil store-types create RFJKS
    ```

* **Create RFJKS manually in the Command UI**:
    <details><summary>Create RFJKS manually in the Command UI</summary>

    Create a store type called `RFJKS` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFJKS | Display name for the store type (may be customized) |
    | Short Name | RFJKS | Short display name for the store type |
    | Capability | RFJKS | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFJKS Basic Tab](docsource/images/RFJKS-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Required | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFJKS Advanced Tab](docsource/images/RFJKS-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |

    The Custom Fields tab should look like this:

    ![RFJKS Custom Fields Tab](docsource/images/RFJKS-custom-fields-store-type-dialog.png)



    </details>
</details>

<details><summary>RFPEM</summary>


* **Create RFPEM using kfutil**:

    ```shell
    # RFPEM
    kfutil store-types create RFPEM
    ```

* **Create RFPEM manually in the Command UI**:
    <details><summary>Create RFPEM manually in the Command UI</summary>

    Create a store type called `RFPEM` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFPEM | Display name for the store type (may be customized) |
    | Short Name | RFPEM | Short display name for the store type |
    | Capability | RFPEM | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFPEM Basic Tab](docsource/images/RFPEM-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Forbidden | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFPEM Advanced Tab](docsource/images/RFPEM-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |
    | IsTrustStore | Trust Store | Bool | false |  | The IsTrustStore field should contain a boolean value ('true' or 'false') indicating whether the store will be identified as a trust store, which can hold multiple certificates without private keys. Example: 'true' for a trust store or 'false' for a store with a single certificate and private key. |
    | IncludesChain | Store Includes Chain | Bool | false |  | The IncludesChain field should contain a boolean value ('true' or 'false') indicating whether the certificate store includes the full certificate chain along with the end entity certificate. Example: 'true' to include the full chain or 'false' to exclude it. |
    | SeparatePrivateKeyFilePath | Separate Private Key File Location | String |  |  | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.pem'. |
    | IsRSAPrivateKey | Is RSA Private Key | Bool | false |  | The IsRSAPrivateKey field should contain a boolean value ('true' or 'false') indicating whether the private key is in PKCS#1 RSA format. Example: 'true' for a PKCS#1 RSA private key or 'false' for a PKCS#8 private key. |
    | IgnorePrivateKeyOnInventory | Ignore Private Key On Inventory | Bool | false |  | The IgnorePrivateKeyOnInventory field should contain a boolean value ('true' or 'false') indicating whether to ignore the private key during inventory, which will make the store inventory-only and return all certificates without private key entries. Example: 'true' to ignore the private key or 'false' to include it. |

    The Custom Fields tab should look like this:

    ![RFPEM Custom Fields Tab](docsource/images/RFPEM-custom-fields-store-type-dialog.png)



    </details>
</details>

<details><summary>RFPkcs12</summary>


* **Create RFPkcs12 using kfutil**:

    ```shell
    # RFPkcs12
    kfutil store-types create RFPkcs12
    ```

* **Create RFPkcs12 manually in the Command UI**:
    <details><summary>Create RFPkcs12 manually in the Command UI</summary>

    Create a store type called `RFPkcs12` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFPkcs12 | Display name for the store type (may be customized) |
    | Short Name | RFPkcs12 | Short display name for the store type |
    | Capability | RFPkcs12 | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFPkcs12 Basic Tab](docsource/images/RFPkcs12-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Required | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFPkcs12 Advanced Tab](docsource/images/RFPkcs12-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |

    The Custom Fields tab should look like this:

    ![RFPkcs12 Custom Fields Tab](docsource/images/RFPkcs12-custom-fields-store-type-dialog.png)



    </details>
</details>

<details><summary>RFDER</summary>


* **Create RFDER using kfutil**:

    ```shell
    # RFDER
    kfutil store-types create RFDER
    ```

* **Create RFDER manually in the Command UI**:
    <details><summary>Create RFDER manually in the Command UI</summary>

    Create a store type called `RFDER` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFDER | Display name for the store type (may be customized) |
    | Short Name | RFDER | Short display name for the store type |
    | Capability | RFDER | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFDER Basic Tab](docsource/images/RFDER-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Forbidden | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFDER Advanced Tab](docsource/images/RFDER-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |
    | SeparatePrivateKeyFilePath | Separate Private Key File Location | String |  |  | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.der'. |

    The Custom Fields tab should look like this:

    ![RFDER Custom Fields Tab](docsource/images/RFDER-custom-fields-store-type-dialog.png)



    </details>
</details>

<details><summary>RFKDB</summary>


* **Create RFKDB using kfutil**:

    ```shell
    # RFKDB
    kfutil store-types create RFKDB
    ```

* **Create RFKDB manually in the Command UI**:
    <details><summary>Create RFKDB manually in the Command UI</summary>

    Create a store type called `RFKDB` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFKDB | Display name for the store type (may be customized) |
    | Short Name | RFKDB | Short display name for the store type |
    | Capability | RFKDB | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFKDB Basic Tab](docsource/images/RFKDB-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Required | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFKDB Advanced Tab](docsource/images/RFKDB-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |

    The Custom Fields tab should look like this:

    ![RFKDB Custom Fields Tab](docsource/images/RFKDB-custom-fields-store-type-dialog.png)



    </details>
</details>

<details><summary>RFORA</summary>


* **Create RFORA using kfutil**:

    ```shell
    # RFORA
    kfutil store-types create RFORA
    ```

* **Create RFORA manually in the Command UI**:
    <details><summary>Create RFORA manually in the Command UI</summary>

    Create a store type called `RFORA` with the attributes in the tables below:

    #### Basic Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Name | RFORA | Display name for the store type (may be customized) |
    | Short Name | RFORA | Short display name for the store type |
    | Capability | RFORA | Store type name orchestrator will register with. Check the box to allow entry of value |
    | Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
    | Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
    | Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
    | Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
    | Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
    | Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
    | Needs Server | ✅ | Determines if a target server name is required when creating store |
    | Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
    | Uses PowerShell |  | Determines if underlying implementation is PowerShell |
    | Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
    | Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

    The Basic tab should look like this:

    ![RFORA Basic Tab](docsource/images/RFORA-basic-store-type-dialog.png)

    #### Advanced Tab
    | Attribute | Value | Description |
    | --------- | ----- | ----- |
    | Supports Custom Alias | Required | Determines if an individual entry within a store can have a custom Alias. |
    | Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
    | PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

    The Advanced tab should look like this:

    ![RFORA Advanced Tab](docsource/images/RFORA-advanced-store-type-dialog.png)

    #### Custom Fields Tab
    Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

    | Name | Display Name | Type | Default Value/Options | Required | Description |
    | ---- | ------------ | ---- | --------------------- | -------- | ----------- |
    | LinuxFilePermissionsOnStoreCreation | Linux File Permissions on Store Creation | String |  |  | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |
    | LinuxFileOwnerOnStoreCreation | Linux File Owner on Store Creation | String |  |  | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |
    | SudoImpersonatingUser | Sudo Impersonating User | String |  |  | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |
    | WorkFolder | Location to use for creation/removal of work files | String |  | ✅ | The WorkFolder field should contain the path on the managed server where temporary work files can be created, modified, and deleted during Inventory and Management jobs. Example: '/path/to/workfolder'. |

    The Custom Fields tab should look like this:

    ![RFORA Custom Fields Tab](docsource/images/RFORA-custom-fields-store-type-dialog.png)



    </details>
</details>


## Installation

1. **Download the latest Remote File Universal Orchestrator extension from GitHub.** 

    On the [Remote File Universal Orchestrator extension GitHub version page](https://github.com/Keyfactor/remote-file-orchestrator/releases/latest), click the `remote-file-orchestrator` asset to download the zip archive. Unzip the archive containing extension assemblies to a known location.

2. **Locate the Universal Orchestrator extensions directory.**

    * **Default on Windows** - `C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions`
    * **Default on Linux** - `/opt/keyfactor/orchestrator/extensions`
    
3. **Create a new directory for the Remote File Universal Orchestrator extension inside the extensions directory.**
        
    Create a new directory called `remote-file-orchestrator`.
    > The directory name does not need to match any names used elsewhere; it just has to be unique within the extensions directory.

4. **Copy the contents of the downloaded and unzipped assemblies from __step 2__ to the `remote-file-orchestrator` directory.**

5. **Restart the Universal Orchestrator service.**

    Refer to [Starting/Restarting the Universal Orchestrator service](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/StarttheService.htm).



> The above installation steps can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions).



## Defining Certificate Stores

The Remote File Universal Orchestrator extension implements 6 Certificate Store Types, each of which implements different functionality. Refer to the individual instructions below for each Certificate Store Type that you deemed necessary for your use case from the installation section.

<details><summary>RFJKS</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFJKS" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The IP address or DNS of the server hosting the certificate store.  For more information, see [Client Machine ](#client-machine-instructions) | |
        | Store Path | The full path and file name, including file extension if one exists where the certificate store file is located.  For Linux orchestrated servers, StorePath will begin with a forward slash (i.e. /folder/path/storename.ext).  For Windows orchestrated servers, it should begin with a drive letter (i.e. c:\folder\path\storename.ext). | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFJKS` certificates. Specifically, one with the `RFJKS` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFJKS certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFJKS --outpath RFJKS.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFJKS" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The IP address or DNS of the server hosting the certificate store.  For more information, see [Client Machine ](#client-machine-instructions) | |
        | Store Path | The full path and file name, including file extension if one exists where the certificate store file is located.  For Linux orchestrated servers, StorePath will begin with a forward slash (i.e. /folder/path/storename.ext).  For Windows orchestrated servers, it should begin with a drive letter (i.e. c:\folder\path\storename.ext). | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFJKS` certificates. Specifically, one with the `RFJKS` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFJKS --file RFJKS.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>

<details><summary>RFPEM</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFPEM" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.ext) for Windows orchestrated servers. Example: '/folder/path/storename.pem' or 'c:\folder\path\storename.pem'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFPEM` certificates. Specifically, one with the `RFPEM` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | IsTrustStore | The IsTrustStore field should contain a boolean value ('true' or 'false') indicating whether the store will be identified as a trust store, which can hold multiple certificates without private keys. Example: 'true' for a trust store or 'false' for a store with a single certificate and private key. |  |
        | IncludesChain | The IncludesChain field should contain a boolean value ('true' or 'false') indicating whether the certificate store includes the full certificate chain along with the end entity certificate. Example: 'true' to include the full chain or 'false' to exclude it. |  |
        | SeparatePrivateKeyFilePath | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.pem'. |  |
        | IsRSAPrivateKey | The IsRSAPrivateKey field should contain a boolean value ('true' or 'false') indicating whether the private key is in PKCS#1 RSA format. Example: 'true' for a PKCS#1 RSA private key or 'false' for a PKCS#8 private key. |  |
        | IgnorePrivateKeyOnInventory | The IgnorePrivateKeyOnInventory field should contain a boolean value ('true' or 'false') indicating whether to ignore the private key during inventory, which will make the store inventory-only and return all certificates without private key entries. Example: 'true' to ignore the private key or 'false' to include it. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFPEM certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFPEM --outpath RFPEM.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFPEM" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.ext) for Windows orchestrated servers. Example: '/folder/path/storename.pem' or 'c:\folder\path\storename.pem'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFPEM` certificates. Specifically, one with the `RFPEM` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | IsTrustStore | The IsTrustStore field should contain a boolean value ('true' or 'false') indicating whether the store will be identified as a trust store, which can hold multiple certificates without private keys. Example: 'true' for a trust store or 'false' for a store with a single certificate and private key. |  |
        | IncludesChain | The IncludesChain field should contain a boolean value ('true' or 'false') indicating whether the certificate store includes the full certificate chain along with the end entity certificate. Example: 'true' to include the full chain or 'false' to exclude it. |  |
        | SeparatePrivateKeyFilePath | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.pem'. |  |
        | IsRSAPrivateKey | The IsRSAPrivateKey field should contain a boolean value ('true' or 'false') indicating whether the private key is in PKCS#1 RSA format. Example: 'true' for a PKCS#1 RSA private key or 'false' for a PKCS#8 private key. |  |
        | IgnorePrivateKeyOnInventory | The IgnorePrivateKeyOnInventory field should contain a boolean value ('true' or 'false') indicating whether to ignore the private key during inventory, which will make the store inventory-only and return all certificates without private key entries. Example: 'true' to ignore the private key or 'false' to include it. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFPEM --file RFPEM.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>

<details><summary>RFPkcs12</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFPkcs12" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.p12) for Windows orchestrated servers. Example: '/folder/path/storename.p12' or 'c:\folder\path\storename.p12'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFPkcs12` certificates. Specifically, one with the `RFPkcs12` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFPkcs12 certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFPkcs12 --outpath RFPkcs12.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFPkcs12" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.p12) for Windows orchestrated servers. Example: '/folder/path/storename.p12' or 'c:\folder\path\storename.p12'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFPkcs12` certificates. Specifically, one with the `RFPkcs12` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFPkcs12 --file RFPkcs12.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>

<details><summary>RFDER</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFDER" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.der) for Windows orchestrated servers. Example: '/folder/path/storename.der' or 'c:\folder\path\storename.der'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFDER` certificates. Specifically, one with the `RFDER` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | SeparatePrivateKeyFilePath | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.der'. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFDER certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFDER --outpath RFDER.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFDER" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.der) for Windows orchestrated servers. Example: '/folder/path/storename.der' or 'c:\folder\path\storename.der'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFDER` certificates. Specifically, one with the `RFDER` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | SeparatePrivateKeyFilePath | The SeparatePrivateKeyFilePath field should contain the full path and file name where the separate private key file will be stored if it is to be kept outside the main certificate file. Example: '/path/to/privatekey.der'. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFDER --file RFDER.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>

<details><summary>RFKDB</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFKDB" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.kdb) for Windows orchestrated servers. Example: '/folder/path/storename.kdb' or 'c:\folder\path\storename.kdb'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFKDB` certificates. Specifically, one with the `RFKDB` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFKDB certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFKDB --outpath RFKDB.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFKDB" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name, including file extension if applicable, beginning with a forward slash (/) for Linux orchestrated servers or a drive letter (i.e., c:\folder\path\storename.kdb) for Windows orchestrated servers. Example: '/folder/path/storename.kdb' or 'c:\folder\path\storename.kdb'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFKDB` certificates. Specifically, one with the `RFKDB` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFKDB --file RFKDB.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>

<details><summary>RFORA</summary>


* **Manually with the Command UI**

    <details><summary>Create Certificate Stores manually in the UI</summary>

    1. **Navigate to the _Certificate Stores_ page in Keyfactor Command.**

        Log into Keyfactor Command, toggle the _Locations_ dropdown, and click _Certificate Stores_.

    2. **Add a Certificate Store.**

        Click the Add button to add a new Certificate Store. Use the table below to populate the **Attributes** in the **Add** form.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFORA" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name of the Oracle Wallet, including the 'eWallet.p12' file name by convention. Example: '/path/to/eWallet.p12' or 'c:\path\to\eWallet.p12'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFORA` certificates. Specifically, one with the `RFORA` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | WorkFolder | The WorkFolder field should contain the path on the managed server where temporary work files can be created, modified, and deleted during Inventory and Management jobs. Example: '/path/to/workfolder'. |  |

    </details>

* **Using kfutil**
    
    <details><summary>Create Certificate Stores with kfutil</summary>
    
    1. **Generate a CSV template for the RFORA certificate store**

        ```shell
        kfutil stores import generate-template --store-type-name RFORA --outpath RFORA.csv
        ```
    2. **Populate the generated CSV file**

        Open the CSV file, and reference the table below to populate parameters for each **Attribute**.
        | Attribute | Description | Attribute is PAM Eligible |
        | --------- | ----------- | ------------------------- |
        | Category | Select "RFORA" or the customized certificate store name from the previous step. | |
        | Container | Optional container to associate certificate store with. | |
        | Client Machine | The Client Machine field should contain the DNS name or IP address of the remote orchestrated server for Linux orchestrated servers, formatted as a URL (protocol://dns-or-ip:port) for Windows orchestrated servers, or '1.1.1.1|LocalMachine' for local agents. Example: 'https://myserver.mydomain.com:5986' or '1.1.1.1|LocalMachine' for local access. | |
        | Store Path | The Store Path field should contain the full path and file name of the Oracle Wallet, including the 'eWallet.p12' file name by convention. Example: '/path/to/eWallet.p12' or 'c:\path\to\eWallet.p12'. | |
        | Orchestrator | Select an approved orchestrator capable of managing `RFORA` certificates. Specifically, one with the `RFORA` capability. | |
        | LinuxFilePermissionsOnStoreCreation | The LinuxFilePermissionsOnStoreCreation field should contain a three-digit value between 000 and 777 representing the Linux file permissions to be set for the certificate store upon creation. Example: '600' or '755'. |  |
        | LinuxFileOwnerOnStoreCreation | The LinuxFileOwnerOnStoreCreation field should contain a valid user ID recognized by the destination Linux server, optionally followed by a colon and a group ID if the group owner differs. Example: 'userID' or 'userID:groupID'. |  |
        | SudoImpersonatingUser | The SudoImpersonatingUser field should contain a valid user ID to impersonate using sudo on the destination Linux server. Example: 'impersonatedUserID'. |  |
        | WorkFolder | The WorkFolder field should contain the path on the managed server where temporary work files can be created, modified, and deleted during Inventory and Management jobs. Example: '/path/to/workfolder'. |  |

    3. **Import the CSV file to create the certificate stores** 

        ```shell
        kfutil stores import csv --store-type-name RFORA --file RFORA.csv
        ```
    </details>

> The content in this section can be supplimented by the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store).


</details>


## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).