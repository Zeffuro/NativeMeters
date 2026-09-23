using System;
using System.Linq;
using System.Text;
using Lumina.Text.Expressions;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Services.Internal;

internal sealed class ActionTooltipText(uint jobId, byte level)
{
    public string? Read(ReadOnlySeString value)
    {
        var text = new StringBuilder();
        return Append(text, value) ? text.ToString() : null;
    }

    private bool Append(StringBuilder text, ReadOnlySeString value)
    {
        foreach (var payload in value)
        {
            if (payload.Type == ReadOnlySePayloadType.Text)
            {
                text.Append(Encoding.UTF8.GetString(payload.Body.Span));
                continue;
            }

            if (payload.Type != ReadOnlySePayloadType.Macro) return false;

            switch (payload.MacroCode)
            {
                case MacroCode.NewLine:
                    text.Append('\n');
                    break;
                case MacroCode.NonBreakingSpace:
                    text.Append(' ');
                    break;
                case MacroCode.Hyphen:
                    text.Append('-');
                    break;
                case MacroCode.If:
                    if (!payload.TryGetExpression(out var condition, out var yes, out var no) ||
                        !TryGetNumber(condition, out var test) || !AppendExpression(text, test != 0 ? yes : no)) return false;
                    break;
                case MacroCode.Switch:
                    var branches = payload.ToArray();
                    if (branches.Length < 2 || !TryGetNumber(branches[0], out var choice) ||
                        choice < 1 || choice >= branches.Length || !AppendExpression(text, branches[choice])) return false;
                    break;
                case MacroCode.Num:
                    if (!payload.TryGetExpression(out var expression) || !TryGetNumber(expression, out var number)) return false;
                    text.Append(number);
                    break;
                case MacroCode.String:
                case MacroCode.Caps:
                case MacroCode.Head:
                case MacroCode.HeadAll:
                case MacroCode.Lower:
                case MacroCode.LowerHead:
                    if (!payload.TryGetExpression(out var nested) || !AppendExpression(text, nested)) return false;
                    break;
                case MacroCode.SoftHyphen:
                case MacroCode.Color:
                case MacroCode.EdgeColor:
                case MacroCode.ShadowColor:
                case MacroCode.ColorType:
                case MacroCode.EdgeColorType:
                case MacroCode.Bold:
                case MacroCode.Italic:
                case MacroCode.Edge:
                case MacroCode.Shadow:
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private bool AppendExpression(StringBuilder text, ReadOnlySeExpression expression)
    {
        if (expression.TryGetString(out var value)) return Append(text, value);
        if (!TryGetNumber(expression, out var number)) return false;

        text.Append(number);
        return true;
    }

    private bool TryGetNumber(ReadOnlySeExpression expression, out int value)
    {
        if (expression.TryGetInt(out value)) return true;

        if (expression.TryGetParameterExpression(out var parameter, out var operand) &&
            parameter == (byte)ExpressionType.GlobalNumber && TryGetNumber(operand, out var index))
        {
            switch (index)
            {
                case 68: value = (int)jobId; return true;
                case 69:
                case 72: value = level; return true;
                default: return false;
            }
        }

        if (!expression.TryGetBinaryExpression(out var operation, out var left, out var right) ||
            !TryGetNumber(left, out var a) || !TryGetNumber(right, out var b)) return false;

        bool result;
        switch ((ExpressionType)operation)
        {
            case ExpressionType.GreaterThanOrEqualTo: result = a >= b; break;
            case ExpressionType.GreaterThan: result = a > b; break;
            case ExpressionType.LessThanOrEqualTo: result = a <= b; break;
            case ExpressionType.LessThan: result = a < b; break;
            case ExpressionType.Equal: result = a == b; break;
            case ExpressionType.NotEqual: result = a != b; break;
            default: return false;
        }

        value = result ? 1 : 0;
        return true;
    }
}
