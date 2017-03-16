﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.Utils
{
    public class MuxerCommandResolver : ICommandResolver
    {
        public CommandSpec Resolve(CommandResolverArguments commandResolverArguments)
        {
            if (commandResolverArguments.CommandName == Muxer.MuxerName)
            {
                var muxer = new Muxer();
                var escapedArgs = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(
                    commandResolverArguments.CommandArguments.OrEmptyIfNull());
                return new CommandSpec(muxer.MuxerPath, escapedArgs, CommandResolutionStrategy.RootedPath);
            }
            return null;
        }
    }
}
