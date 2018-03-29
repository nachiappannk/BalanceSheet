using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Nachiappan.BalanceSheet
{
    public class FormatableDataGrid : DataGrid
    {
        public FormatableDataGrid() : base()
        {
            this.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var name = GetColumnName(e.PropertyDescriptor);
            if (!string.IsNullOrEmpty(name))
            {
                e.Column.Header = name;
            }
            var displayFormat = GetColumnFormat(e.PropertyDescriptor);
            if (!string.IsNullOrEmpty(displayFormat))
            {
                ((DataGridTextColumn)e.Column).Binding.StringFormat = displayFormat;
            }
            var isEditable = IsColumnEditable(e.PropertyDescriptor);
            e.Column.IsReadOnly = !isEditable;
            if (isEditable)
            {
                e.Column.Header = e.Column.Header + Environment.NewLine + "(Editable)";
                e.Column.CellStyle = new Style(typeof(DataGridCell));
                e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.PeachPuff)));
            }
        }


        public static bool IsObjectOfType<T>(object obj) where T : class
        {
            var propertyDescriptor = obj as T;
            return propertyDescriptor != null;
        }

        public static T GetObjectAsType<T>(object obj) where T : class
        {
            var propertyDescriptor = obj as T;
            return propertyDescriptor;
        }

        public static bool TryGetAttribute<T>(PropertyDescriptor propertyDescriptor, out T t) where T : class
        {
            var attribute = propertyDescriptor.Attributes[typeof(T)] as T;
            t = attribute;
            return attribute != null;
        }

        public static bool TryGetAttribute<T>(PropertyInfo propertyInfo, out T t) where T : class
        {
            var attributes = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (attributes.Length == 0)
            {
                t = default(T);
                return false;
            }
            t = attributes.ElementAt(0) as T;
            return true;
        }

        public static string GetColumnName(object descriptor)
        {

            if (IsObjectOfType<PropertyDescriptor>(descriptor))
            {
                var propertyDescriptor = GetObjectAsType<PropertyDescriptor>(descriptor);
                DisplayNameAttribute displayNameAttribute = null;
                return TryGetAttribute(propertyDescriptor, out displayNameAttribute) ? displayNameAttribute.DisplayName : string.Empty;
            }

            if (IsObjectOfType<PropertyInfo>(descriptor))
            {
                var propertyInfo = GetObjectAsType<PropertyInfo>(descriptor);
                DisplayNameAttribute displayNameAttribute = null;
                return TryGetAttribute(propertyInfo, out displayNameAttribute) ? displayNameAttribute.DisplayName : string.Empty;
            }
            return string.Empty;
        }

        public static bool IsColumnEditable(object descriptor)
        {

            if (IsObjectOfType<PropertyDescriptor>(descriptor))
            {
                var propertyDescriptor = GetObjectAsType<PropertyDescriptor>(descriptor);
                EditableAttribute editableAttribute = null;
                return TryGetAttribute(propertyDescriptor, out editableAttribute) && editableAttribute.AllowEdit;
            }

            if (IsObjectOfType<PropertyInfo>(descriptor))
            {
                var propertyInfo = GetObjectAsType<PropertyInfo>(descriptor);
                EditableAttribute editableAttribute = null;
                return TryGetAttribute(propertyInfo, out editableAttribute) && editableAttribute.AllowEdit;
            }
            return false;
        }

        public static string GetColumnFormat(object descriptor)
        {
            if (IsObjectOfType<PropertyDescriptor>(descriptor))
            {
                var propertyDescriptor = GetObjectAsType<PropertyDescriptor>(descriptor);
                DisplayFormatAttribute displayFormatAttribute = null;
                return TryGetAttribute(propertyDescriptor, out displayFormatAttribute) ? displayFormatAttribute.DataFormatString : string.Empty;
            }

            if (IsObjectOfType<PropertyInfo>(descriptor))
            {
                var propertyInfo = GetObjectAsType<PropertyInfo>(descriptor);
                DisplayFormatAttribute displayFormatAttribute = null;
                return TryGetAttribute(propertyInfo, out displayFormatAttribute) ? displayFormatAttribute.DataFormatString : string.Empty;
            }
            return string.Empty;
        }
    }
}