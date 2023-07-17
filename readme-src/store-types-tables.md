
### RFJKS Store Type
#### kfutil Create RFJKS Store Type
The following commands can be used with [kfutil](https://github.com/Keyfactor/kfutil). Please refer to the kfutil documentation for more information on how to use the tool to interact w/ Keyfactor Command.

```
bash
kfutil login
kfutil store - types create--name RFJKS 
```

#### UI Configuration
##### UI Basic Tab
| Field Name              | Required | Value                                     |
|-------------------------|----------|-------------------------------------------|
| Name                    | &check;  | RFJKS                          |
| ShortName               | &check;  | RFJKS                          |
| Custom Capability       |          | Unchecked [ ]                             |
| Supported Job Types     | &check;  | Inventory,Add,Create,Discovery,Remove     |
| Needs Server            | &check;  | Checked [x]                         |
| Blueprint Allowed       |          | Unchecked [ ]                       |
| Uses PowerShell         |          | Unchecked [ ]                             |
| Requires Store Password |          | Checked [x]                          |
| Supports Entry Password |          | Unchecked [ ]                         |
      
![rfjks_basic.png](docs%2Fscreenshots%2Fstore_types%2Frfjks_basic.png)

##### UI Advanced Tab
| Field Name            | Required | Value                 |
|-----------------------|----------|-----------------------|
| Store Path Type       |          | undefined      |
| Supports Custom Alias |          | Required |
| Private Key Handling  |          | Optional  |
| PFX Password Style    |          | Default   |

![rfjks_advanced.png](docs%2Fscreenshots%2Fstore_types%2Frfjks_advanced.png)

##### UI Custom Fields Tab
| Name           | Display Name         | Type   | Required | Default Value |
| -------------- | -------------------- | ------ | -------- | ------------- |
|LinuxFilePermissionsOnStoreCreation|Linux File Permissions on Store Creation|String||false|
|LinuxFileOwnerOnStoreCreation|Linux File Owner on Store Creation|String||false|


**Entry Parameters:**

Entry parameters are inventoried and maintained for each entry within a certificate store.
They are typically used to support binding of a certificate to a resource.

|Name|Display Name| Type|Default Value|Required When |
|----|------------|-----|-------------|--------------|


### RFPEM Store Type
#### kfutil Create RFPEM Store Type
The following commands can be used with [kfutil](https://github.com/Keyfactor/kfutil). Please refer to the kfutil documentation for more information on how to use the tool to interact w/ Keyfactor Command.

```
bash
kfutil login
kfutil store - types create--name RFPEM 
```

#### UI Configuration
##### UI Basic Tab
| Field Name              | Required | Value                                     |
|-------------------------|----------|-------------------------------------------|
| Name                    | &check;  | RFPEM                          |
| ShortName               | &check;  | RFPEM                          |
| Custom Capability       |          | Unchecked [ ]                             |
| Supported Job Types     | &check;  | Inventory,Add,Create,Discovery,Remove     |
| Needs Server            | &check;  | Checked [x]                         |
| Blueprint Allowed       |          | Unchecked [ ]                       |
| Uses PowerShell         |          | Unchecked [ ]                             |
| Requires Store Password |          | Checked [x]                          |
| Supports Entry Password |          | Unchecked [ ]                         |
      
![rfpem_basic.png](docs%2Fscreenshots%2Fstore_types%2Frfpem_basic.png)

##### UI Advanced Tab
| Field Name            | Required | Value                 |
|-----------------------|----------|-----------------------|
| Store Path Type       |          | undefined      |
| Supports Custom Alias |          | Forbidden |
| Private Key Handling  |          | Optional  |
| PFX Password Style    |          | Default   |

![rfpem_advanced.png](docs%2Fscreenshots%2Fstore_types%2Frfpem_advanced.png)

##### UI Custom Fields Tab
| Name           | Display Name         | Type   | Required | Default Value |
| -------------- | -------------------- | ------ | -------- | ------------- |
|LinuxFilePermissionsOnStoreCreation|Linux File Permissions on Store Creation|String||false|
|LinuxFileOwnerOnStoreCreation|Linux File Owner on Store Creation|String||false|
|IsTrustStore|Trust Store|Bool|false|false|
|IncludesChain|Store Includes Chain|Bool|false|false|
|SeparatePrivateKeyFilePath|Separate Private Key File Location|String||false|
|IsRSAPrivateKey|Is RSA Private Key|Bool|false|false|


**Entry Parameters:**

Entry parameters are inventoried and maintained for each entry within a certificate store.
They are typically used to support binding of a certificate to a resource.

|Name|Display Name| Type|Default Value|Required When |
|----|------------|-----|-------------|--------------|


### RFPkcs12 Store Type
#### kfutil Create RFPkcs12 Store Type
The following commands can be used with [kfutil](https://github.com/Keyfactor/kfutil). Please refer to the kfutil documentation for more information on how to use the tool to interact w/ Keyfactor Command.

```
bash
kfutil login
kfutil store - types create--name RFPkcs12 
```

#### UI Configuration
##### UI Basic Tab
| Field Name              | Required | Value                                     |
|-------------------------|----------|-------------------------------------------|
| Name                    | &check;  | RFPkcs12                          |
| ShortName               | &check;  | RFPkcs12                          |
| Custom Capability       |          | Unchecked [ ]                             |
| Supported Job Types     | &check;  | Inventory,Add,Create,Discovery,Remove     |
| Needs Server            | &check;  | Checked [x]                         |
| Blueprint Allowed       |          | Unchecked [ ]                       |
| Uses PowerShell         |          | Unchecked [ ]                             |
| Requires Store Password |          | Checked [x]                          |
| Supports Entry Password |          | Unchecked [ ]                         |
      
![rfpkcs12_basic.png](docs%2Fscreenshots%2Fstore_types%2Frfpkcs12_basic.png)

##### UI Advanced Tab
| Field Name            | Required | Value                 |
|-----------------------|----------|-----------------------|
| Store Path Type       |          | undefined      |
| Supports Custom Alias |          | Required |
| Private Key Handling  |          | Optional  |
| PFX Password Style    |          | Default   |

![rfpkcs12_advanced.png](docs%2Fscreenshots%2Fstore_types%2Frfpkcs12_advanced.png)

##### UI Custom Fields Tab
| Name           | Display Name         | Type   | Required | Default Value |
| -------------- | -------------------- | ------ | -------- | ------------- |
|LinuxFilePermissionsOnStoreCreation|Linux File Permissions on Store Creation|String||false|
|LinuxFileOwnerOnStoreCreation|Linux File Owner on Store Creation|String||false|


**Entry Parameters:**

Entry parameters are inventoried and maintained for each entry within a certificate store.
They are typically used to support binding of a certificate to a resource.

|Name|Display Name| Type|Default Value|Required When |
|----|------------|-----|-------------|--------------|


### RFDER Store Type
#### kfutil Create RFDER Store Type
The following commands can be used with [kfutil](https://github.com/Keyfactor/kfutil). Please refer to the kfutil documentation for more information on how to use the tool to interact w/ Keyfactor Command.

```
bash
kfutil login
kfutil store - types create--name RFDER 
```

#### UI Configuration
##### UI Basic Tab
| Field Name              | Required | Value                                     |
|-------------------------|----------|-------------------------------------------|
| Name                    | &check;  | RFDER                          |
| ShortName               | &check;  | RFDER                          |
| Custom Capability       |          | Unchecked [ ]                             |
| Supported Job Types     | &check;  | Inventory,Add,Create,Discovery,Remove     |
| Needs Server            | &check;  | Checked [x]                         |
| Blueprint Allowed       |          | Unchecked [ ]                       |
| Uses PowerShell         |          | Unchecked [ ]                             |
| Requires Store Password |          | Checked [x]                          |
| Supports Entry Password |          | Unchecked [ ]                         |
      
![rfder_basic.png](docs%2Fscreenshots%2Fstore_types%2Frfder_basic.png)

##### UI Advanced Tab
| Field Name            | Required | Value                 |
|-----------------------|----------|-----------------------|
| Store Path Type       |          | undefined      |
| Supports Custom Alias |          | Forbidden |
| Private Key Handling  |          | Optional  |
| PFX Password Style    |          | Default   |

![rfder_advanced.png](docs%2Fscreenshots%2Fstore_types%2Frfder_advanced.png)

##### UI Custom Fields Tab
| Name           | Display Name         | Type   | Required | Default Value |
| -------------- | -------------------- | ------ | -------- | ------------- |
|LinuxFilePermissionsOnStoreCreation|Linux File Permissions on Store Creation|String||false|
|LinuxFileOwnerOnStoreCreation|Linux File Owner on Store Creation|String||false|
|SeparatePrivateKeyFilePath|Separate Private Key File Location|String||false|


**Entry Parameters:**

Entry parameters are inventoried and maintained for each entry within a certificate store.
They are typically used to support binding of a certificate to a resource.

|Name|Display Name| Type|Default Value|Required When |
|----|------------|-----|-------------|--------------|


### RFKDB Store Type
#### kfutil Create RFKDB Store Type
The following commands can be used with [kfutil](https://github.com/Keyfactor/kfutil). Please refer to the kfutil documentation for more information on how to use the tool to interact w/ Keyfactor Command.

```
bash
kfutil login
kfutil store - types create--name RFKDB 
```

#### UI Configuration
##### UI Basic Tab
| Field Name              | Required | Value                                     |
|-------------------------|----------|-------------------------------------------|
| Name                    | &check;  | RFKDB                          |
| ShortName               | &check;  | RFKDB                          |
| Custom Capability       |          | Unchecked [ ]                             |
| Supported Job Types     | &check;  | Inventory,Add,Create,Discovery,Remove     |
| Needs Server            | &check;  | Checked [x]                         |
| Blueprint Allowed       |          | Unchecked [ ]                       |
| Uses PowerShell         |          | Unchecked [ ]                             |
| Requires Store Password |          | Checked [x]                          |
| Supports Entry Password |          | Unchecked [ ]                         |
      
![rfkdb_basic.png](docs%2Fscreenshots%2Fstore_types%2Frfkdb_basic.png)

##### UI Advanced Tab
| Field Name            | Required | Value                 |
|-----------------------|----------|-----------------------|
| Store Path Type       |          | undefined      |
| Supports Custom Alias |          | Required |
| Private Key Handling  |          | Optional  |
| PFX Password Style    |          | Default   |

![rfkdb_advanced.png](docs%2Fscreenshots%2Fstore_types%2Frfkdb_advanced.png)

##### UI Custom Fields Tab
| Name           | Display Name         | Type   | Required | Default Value |
| -------------- | -------------------- | ------ | -------- | ------------- |
|LinuxFilePermissionsOnStoreCreation|Linux File Permissions on Store Creation|String||false|
|LinuxFileOwnerOnStoreCreation|Linux File Owner on Store Creation|String||false|


**Entry Parameters:**

Entry parameters are inventoried and maintained for each entry within a certificate store.
They are typically used to support binding of a certificate to a resource.

|Name|Display Name| Type|Default Value|Required When |
|----|------------|-----|-------------|--------------|

