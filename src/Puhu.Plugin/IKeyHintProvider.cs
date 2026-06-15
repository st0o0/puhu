namespace Puhu.Plugin;

public interface IKeyHintProvider
{
    string[] GetKeyHints();
    bool ShowGlobalHints => true;
}
