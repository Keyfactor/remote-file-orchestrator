// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    class RemoteFileException : ApplicationException
    {
        public RemoteFileException(string message) : base(message)
        { }

        public RemoteFileException(string message, Exception ex) : base(message, ex)
        { }

        public static string FlattenExceptionMessages(Exception ex, string message)
        {
            message += ex.Message + Environment.NewLine;
            if (ex.InnerException != null)
                message = FlattenExceptionMessages(ex.InnerException, message);

            return message;
        }
    }
}
