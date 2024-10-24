## Overview

The RFPkcs12 store type can be used to manage any PKCS#12 compliant file format INCLUDING java keystores of type PKCS12.

Use cases supported:
1. One-to-many trust entries -  A trust entry is defined as a single certificate without a private key in a certificate store.  Each trust entry MUST BE identified with a custom friendly name/alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate MUST BE identified with a custom friendly name/alias.
3. A mix of trust and key entries.  Each entry MUST BE identified with a custom friendly name/alias.
4. Single certificate stores with a blank/missing friendly name/alias.  Any management add job will replace the current certificate entry and will keep the friendly name/alias blank.  The Keyfactor Command certificate store will show the current certificate thumbprint as the entry's alias.

Use cases not supported:
1. Multiple key and/or trust entries with a mix of existing and non existing friendly names/aliases.
2. Multiple key and/or trust entries with blank friendly names/aliases
