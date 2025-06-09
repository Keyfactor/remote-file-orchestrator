## Overview

The `RFORA` store type can be used to manage `PKCS12` Oracle Wallets. 

> NOTE: This should work for `PKCS12` Oracle Wallets installed on both Windows and Linux servers, this has only been tested on wallets installed on Windows.  
> NOTE: When entering the Store Path for an Oracle Wallet in Keyfactor Command, make sure to INCLUDE the `eWallet.p12` file name that by convention is the name of the `PKCS12` wallet file that gets created.

### Supported use cases
1. One-to-many trust entries - A trust entry is defined as a single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate is identified with a custom alias.
3. A mix of trust and key entries.
