﻿using System;
using System.Collections.Generic;

using OrbitalShell.Lib;

using static OrbitalShell.Component.CommandLine.Parsing.Variable.VariableSyntax;

namespace OrbitalShell.Component.Shell.Data
{
    public class DataRegistry
    {
        readonly Dictionary<string, object> _objects
            = new();

        public readonly DataObject RootObject = new("root");

        public List<IDataObject> GetDataValues() => RootObject.GetAttributes();

        public void Set(string path, object value = null, bool isReadOnly = false, Type type = null)
        {
            var p = SplitPath(path);
            var valueObj = RootObject.Set(p, value, isReadOnly, type);
            if (RootObject.Get(p, out _) && !_objects.ContainsKey(path))
                _objects.AddOrReplace(path, valueObj);
        }

        public void Unset(string path)
        {
            RootObject.Unset(SplitPath(path));
            if (_objects.ContainsKey(path))
                _objects.Remove(path);
        }

        public bool Get(string path, out object data)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "";

            if (_objects.TryGetValue(path, out var value))
            {
                data = value;
                return true;
            }
            if (RootObject.Get(SplitPath(path), out var sdata))
            {
                _objects.AddOrReplace(path, sdata);
                data = sdata;
                return true;
            }
            data = null;
            return false;
        }

        public bool GetPathOwner(string path, out object data)
            => RootObject.GetPathOwner(SplitPath(path), out data);

    }
}
