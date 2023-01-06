#!/bin/bash
source .env
source ~/GolandProjects/kfutil/.env
echo install.sh --force --username "${KEYFACTOR_USERNAME}" --password "${KEYFACTOR_PASSWORD}" --url "https://${KEYFACTOR_HOSTNAME}/KeyfactorAgents" --orchestrator-name "${UO_NAME_PREFIX}${HOSTNAME}${UO_NAME_SUFFIX}" --capabilities all

mono --debugger-agent=address=0.0.0.0:50000,server=y,transport=dt_socket --debug /opt/keyfactor/orchestrator/Orchestrator.dll
mono --debugger-agent=address=0.0.0.0:50000,server=y,transport=dt_socket --debug /opt/keyfactor/orchestrator/extensions/RemoteFileOrchestrator/RemoteFile.dll