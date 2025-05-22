## Overview

The `RFPEM` store type can be used to manage `PEM` encoded files.

### Supported use cases
1. Trust stores - A file with one-to-many certificates (no private keys, no certificate chains).
2. Single certificate stores with private key in the file.
3. Single certificate stores with certificate chain and private key in the file.
4. Single certificate stores with private key in an external file.
5. Single certificate stores with certificate chain in the file and private key in an external file

### Additional Considerations and Limitations
- `PEM` stores may only have one private key (internal or external) associated with the store, as only one certificate/chain/private key combination can be stored in a PEM store supported by `RFPEM`. 
- Private keys will be stored in encrypted or unencrypted `PKCS#8` format (`BEGIN [ENCRYPTED] PRIVATE KEY`) based on the Store Password set on the Keyfactor Command Certificate Store unless managing a `PEM` store that currently contains a private key in `PKCS#1` format (`BEGIN RSA PRIVATE KEY` or `BEGIN EC PRIVATE KEY`). 
- Store password *MUST* be set to `No Password` if managing a store with a `PKCS#1` private key, as encrypted `PKCS#1` keys are not supported with this integration. 
