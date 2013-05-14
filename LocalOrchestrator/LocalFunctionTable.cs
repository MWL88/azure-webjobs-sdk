﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Executor;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using RunnerHost;
using RunnerInterfaces;
using SimpleBatch;
using SimpleBatch.Client;

namespace Orchestrator
{
    // Provide in-memory settings that can glue an indexer, orchestrator, and execution.
    // Executes via in-memory MethodInfos without azure. 
    // $$$ Merge with any other IFunctionTable, like IndexInMemory
    public class LocalFunctionTable : IFunctionTable
    {
        List<FunctionDefinition> _funcs = new List<FunctionDefinition>();
        List<MethodInfo> _mapping = new List<MethodInfo>();

        // account is for binding parameters. 
        public LocalFunctionTable(CloudStorageAccount account)
        {
            this.Account = account;
            this.AccountConnectionString = Utility.GetConnectionString(account);
        }

        public CloudStorageAccount Account { get; private set; }
        public string AccountConnectionString { get; private set; }

        public MethodInfo GetMethod(string functionShortName)
        {
            foreach (var method in _mapping)
            {
                if (method.Name == functionShortName)
                {
                    return method;
                }
            }
            string msg = string.Format("Can't resolve function '{0}'.", functionShortName);
            throw new InvalidOperationException(msg);
        }

        public FunctionLocation OnApplyLocationInfo(MethodInfo method)
        {
            _mapping.Add(method);

            // Still need account information because blob inputs are relative to these accounts.
            return new MethodInfoFunctionLocation
            {
                AccountConnectionString = this.AccountConnectionString,
                MethodInfo = method
            };
        }

        void IFunctionTable.Add(FunctionDefinition func)
        {
            _funcs.Add(func);
        }

        void IFunctionTable.Delete(FunctionDefinition func)
        {
            string funcString = func.ToString();
            foreach (var x in _funcs)
            {
                if (x.ToString() == funcString)
                {
                    _funcs.Remove(x);
                    return;
                }
            }
        }

        FunctionDefinition[] IFunctionTableLookup.ReadAll()
        {
            return _funcs.ToArray();
        }  

        public DateTime? GetLastExecutionTime(FunctionLocation func)
        {
            return null;
        }
       
        public FunctionDefinition Lookup(string functionId)
        {
            throw new NotImplementedException();
        }
    }
}