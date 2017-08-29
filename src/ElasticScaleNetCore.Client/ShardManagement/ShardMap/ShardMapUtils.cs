// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.SqlDatabase.ElasticScaleNetCore.ShardManagement
{
    /// <summary>
    /// Helper methods related to shard map instantiation.
    /// </summary>
    internal static class ShardMapUtils
    {
        /// <summary>
        /// SqlConnectionStringBuilder property that allows one
        /// to specify the number of reconnect attempts on connection failure 
        /// </summary>
        internal static readonly string ConnectRetryCount = "ConnectRetryCount";

        /// <summary>
        /// SqlConnectionStringBuilder property that allows specifying
        /// active directoty authentication to connect to SQL instance.
        /// </summary>
        internal static readonly string Authentication = "Authentication";

        /// <summary>
        /// String representation of SqlAuthenticationMethod.ActiveDirectoryIntegrated
        /// SqlAuthenticationMethod.ActiveDirectoryIntegrated.ToString() cannot be used 
        /// because it may not be available in the .NET framework version that we are running in
        /// </summary>
        internal static readonly string ActiveDirectoryIntegratedStr = "ActiveDirectoryIntegrated";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ShardMapUtils()
        {
            // Connection resiliency is supported if this SqlClient instance
            // allows setting the retry count on connection failure
            SqlConnectionStringBuilder bldr = new SqlConnectionStringBuilder();
            if (bldr.ContainsKey(ConnectRetryCount))
            {
                IsConnectionResiliencySupported = true;
            }
        }

        /// <summary>
        /// Whether this SqlClient instance supports Connection Resiliency
        /// </summary>
        internal static bool IsConnectionResiliencySupported { get; private set; }

        /// <summary>
        /// Converts IStoreShardMap to ShardMap.
        /// </summary>
        /// <param name="manager">Reference to shard map manager.</param>
        /// <param name="ssm">Storage representation for ShardMap.</param>
        /// <returns>ShardMap object corresponding to storange representation.</returns>
        internal static ShardMap CreateShardMapFromStoreShardMap(
            ShardMapManager manager,
            IStoreShardMap ssm)
        {
            Type typeOfShardMap;
            ConstructorInfo ctInfo;

            switch (ssm.MapType)
            {
                case ShardMapType.List:
                    // Create ListShardMap<TKey>
                    typeOfShardMap = typeof(ListShardMap<>).MakeGenericType(ShardKey.TypeFromShardKeyType(ssm.KeyType));
                    ctInfo = typeOfShardMap.GetTypeInfo().DeclaredConstructors.Single(ct => CheckParamaterMatch(ct, manager, ssm));
                    break;

                default:
                    Debug.Assert(ssm.MapType == ShardMapType.Range);
                    typeOfShardMap = typeof(RangeShardMap<>).MakeGenericType(ShardKey.TypeFromShardKeyType(ssm.KeyType));
                    ctInfo = typeOfShardMap.GetTypeInfo().DeclaredConstructors.Single(ct => CheckParamaterMatch(ct, manager, ssm));
                    break;
            }

            return ctInfo.Invoke(new object[] { manager, ssm }) as ShardMap;
        }

        private static bool CheckParamaterMatch(MethodBase  method, params object[] parameters)
        {
            var methodParameters = method.GetParameters();
            parameters = parameters ?? new object[0];

            if (parameters.Length != methodParameters.Length)
            {
                return false;
            }

            for (var i = 0; i < methodParameters.Length; i++)
            {
                if (!methodParameters[i].ParameterType.GetTypeInfo().IsAssignableFrom(parameters[i].GetType()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
