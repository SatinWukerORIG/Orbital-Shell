﻿using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using static DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.Str;

namespace DotNetConsoleAppToolkit.Console
{
    public static partial class EchoPrimitives
    {
        static Table GetVarsDataTable(List<IDataObject> values)
        {
            var table = new Table();
            table.AddColumns("name", "type", "value");
            table.SetFormat("name", ColorSettings.DarkLabel + "{0}" + Rsf);
            table.SetFormat("type", ColorSettings.Label + "{0}" + Tab + Rsf);
            table.SetHeaderFormat("type", "{0}" + Tab);
            foreach (var value in values)
            {
                if (value == null)
                {
                    table.Rows.Add(DumpAsText(null), DumpAsText(null), DumpAsText(null));
                }
                else
                {
                    var dv = value as DataValue;
                    var valueType = (dv!=null) ? dv.ValueType.Name : value.GetType().Name;
                    var val = (dv!=null) ? DumpAsText(dv.Value, false) : string.Empty;
                    var valnprefix = (dv == null) ? (ColorSettings.Highlight+"[+] ") : "    ";
                    var valnostfix = (dv == null) ? "" : "";

                    table.Rows.Add(
                        valnprefix + value.Name + (value.IsReadOnly ? "(r)" : "") + valnostfix,
                        valueType,
                        DumpAsText(val, false));
                }
            }
            return table;
        }

        public static void Echo(
            this IDataObject dataObject,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context
            )
        {
            var values = dataObject.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));

            var dt = GetVarsDataTable(values);
            dt.Echo( @out, context, true, false);
        }

        public static void Echo(
            this Variables variables,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context
            )
        {
            var values = variables.GetDataValues();
            //var objects = variables.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));
            var dt = GetVarsDataTable(values);
            dt.Echo( @out, context, true, false );
        }

        public static void Echo(
            this DataTable table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,            
            bool noBorders=false,
            bool padLastColumn=true)
        {
            @out.EnableFillLineFromCursor = false;
            @out.HideCur();
            var colLengths = new int[table.Columns.Count];
            foreach ( var rw in table.Rows )
            {
                var cols = ((DataRow)rw).ItemArray;
                for (int i = 0; i < cols.Length; i++)
                {
                    var s = @out.GetPrint(cols[i]?.ToString()) ?? "";
                    colLengths[i] = Math.Max(s.Length, colLengths[i]);
                    colLengths[i] = Math.Max(table.Columns[i].ColumnName.Length, colLengths[i]);
                }
            }
            var colsep = noBorders ? " " : (ColorSettings.TableBorder + " | " + ColorSettings.Default);
            var colseplength = noBorders?0:3;
            var tablewidth = noBorders ? 0 : 3;
            for (int i = 0; i < table.Columns.Count; i++)
                tablewidth += table.Columns[i].ColumnName.PadRight(colLengths[i], ' ').Length + colseplength;
            var line = noBorders ? "" : (ColorSettings.TableBorder + "".PadRight(tablewidth, '-'));

            if (!noBorders) @out.Echoln(line);
            if (noBorders) @out.Echo(Uon);
            for (int i=0;i<table.Columns.Count;i++)
            {
                if (i == 0) @out.Echo(colsep);
                var col = table.Columns[i];
                var colName = (i==table.Columns.Count-1 && !padLastColumn)? col.ColumnName : col.ColumnName.PadRight(colLengths[i], ' ');
                var prfx = (noBorders) ? Uon : "";
                var pofx = (noBorders) ? Tdoff : "";
                @out.Echo(ColorSettings.TableColumnName + prfx + colName + colsep + pofx);
            }
            @out.Echoln();
            if (!noBorders) @out.Echoln(line);

            foreach ( var rw in table.Rows )
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                {
                    @out.EnableFillLineFromCursor = true;
                    @out.ShowCur();
                    @out.Echoln(ColorSettings.Default + "");
                    return;
                }
                var row = (DataRow)rw;
                var arr = row.ItemArray;
                for (int i=0;i<arr.Length;i++)
                {
                    if (i == 0) Out.Echo(colsep);
                    var txt = (arr[i]==null)?"":arr[i].ToString();
                    var l = Out.GetPrint(txt).Length;
                    var spc = (i == arr.Length - 1 && !padLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                    @out.Echo(ColorSettings.Default+txt+spc+colsep);
                }
                @out.Echoln();
            }
            @out.Echoln(line+ColorSettings.Default.ToString());
            @out.ShowCur();
            @out.EnableFillLineFromCursor = true;
        }

        public static void Echo(
            this Table table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            bool noBorders = false,
            bool padLastColumn = true)
        {
            @out.EnableFillLineFromCursor = false;
            @out.HideCur();
            var colLengths = new int[table.Columns.Count];
            foreach (var rw in table.Rows)
            {
                var cols = ((DataRow)rw).ItemArray;
                for (int i = 0; i < cols.Length; i++)
                {
                    var s = @out.GetPrint(table.GetFormatedValue(table.Columns[i].ColumnName, cols[i]?.ToString())) ?? "";
                    colLengths[i] = Math.Max(s.Length, colLengths[i]);
                    var s2 = @out.GetPrint(table.GetFormatedHeader(table.Columns[i].ColumnName)) ?? "";
                    colLengths[i] = Math.Max(s2.Length, colLengths[i]);
                }
            }
            var colsep = noBorders ? " " : (ColorSettings.TableBorder + " | " + ColorSettings.Default);
            var colseplength = noBorders ? 0 : 3;
            var tablewidth = noBorders ? 0 : 3;
            for (int i = 0; i < table.Columns.Count; i++)
                tablewidth += table.Columns[i].ColumnName.PadRight(colLengths[i], ' ').Length + colseplength;
            var line = noBorders ? "" : (ColorSettings.TableBorder + "".PadRight(tablewidth, '-'));

            if (!noBorders) @out.Echoln(line);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i == 0) @out.Echo(colsep);
                var col = table.Columns[i];
                var colName = (i == table.Columns.Count - 1 && !padLastColumn) ?
                    table.GetFormatedHeader(col.ColumnName) 
                    : table.GetFormatedHeader(col.ColumnName).PadRight(colLengths[i], ' ');
                var prfx = (noBorders) ? Uon : "";
                var pofx = (noBorders) ? Tdoff : "";
                @out.Echo(ColorSettings.TableColumnName + prfx + colName + colsep + pofx);
            }
            @out.Echoln();
            if (!noBorders) @out.Echoln(line);

            foreach (var rw in table.Rows)
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                {
                    @out.EnableFillLineFromCursor = true;
                    @out.ShowCur();
                    @out.Echoln(ColorSettings.Default + "");
                    return;
                }
                var row = (DataRow)rw;
                var arr = row.ItemArray;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == 0) Out.Echo(colsep);
                    var txt = (arr[i] == null) ? "" : arr[i].ToString();
                    var fvalue = table.GetFormatedValue(table.Columns[i].ColumnName, txt);
                    var l = Out.GetPrint(fvalue).Length;
                    var spc = (i == arr.Length - 1 && !padLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                    @out.Echo(ColorSettings.Default + fvalue + spc + colsep);
                }
                @out.Echoln();
            }
            @out.Echoln(line + ColorSettings.Default.ToString());
            @out.ShowCur();
            @out.EnableFillLineFromCursor = true;
        }
    }
}
