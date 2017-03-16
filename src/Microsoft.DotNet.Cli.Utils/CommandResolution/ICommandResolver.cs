﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.Utils
{
    public interface ICommandResolver
    {
        CommandSpec Resolve(CommandResolverArguments arguments);
    }
}
