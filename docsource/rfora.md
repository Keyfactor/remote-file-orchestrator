## Overview

The `RFORA` store type can be used to manage `PKCS12` Oracle wallets. Although implemented as a separate store type, Oracle wallets are accessed and managed identically to RFPkcs12 store types.  The file is expected to compatible with the Pkcs#12 standard.

### Supported use cases
1. One-to-many trust entries - A trust entry is defined as a single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate is identified with a custom alias.
3. A mix of trust and key entries.
