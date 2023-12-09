namespace azdiff;

enum ReplaceTextTarget
{
    Name = 1,
    Body = 2,
    Both = Name | Body
}
record ReplaceText(ReplaceTextTarget Target, string Input, string Replacement);