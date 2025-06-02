using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace LiveFeedback.Converters.InputValidators;

public class General
{
    public static void EnsureNumberOnly<T>(object? sender, TextInputEventArgs e) where T : IParsable<T>
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            e.Handled = true; // blocks further processing
            return;
        }

        string text;
        switch (sender)
        {
            case TextBox textBox when textBox.Text != null:
            {
                int caretIndex = textBox.CaretIndex;
                text = textBox.Text.Insert(caretIndex, e.Text).Replace(",", ".");
                break;
            }
            case NumericUpDown numericUpDown when numericUpDown.Text != null:
            {
                TextBox? innerTextBox = numericUpDown
                    .GetVisualDescendants()
                    .OfType<TextBox>()
                    .FirstOrDefault();

                if (innerTextBox?.Text == null)
                {
                    e.Handled = true;
                    return;
                }

                int caretIndex = innerTextBox.CaretIndex;
                text = innerTextBox.Text.Insert(caretIndex, e.Text).Replace(",", ".");
                break;
            }
            default:
                e.Handled = true;
                return;
        }

        if (!IsValidNumber<T>(text))
        {
            e.Handled = true; // break
        }
    }

    public static bool IsValidNumber<T>(string input) where T : IParsable<T>
    {
        try
        {
            _ = T.Parse(input, CultureInfo.CurrentCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsIntegerType<T>() where T : IParsable<T>
    {
        TypeCode code = Type.GetTypeCode(typeof(T));
        switch (code)
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }
}