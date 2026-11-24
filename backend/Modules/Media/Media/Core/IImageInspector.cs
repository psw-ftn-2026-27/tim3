namespace Media.Core;

public interface IImageInspector
{
    ImageHeader? Inspect(byte[] content);
}
