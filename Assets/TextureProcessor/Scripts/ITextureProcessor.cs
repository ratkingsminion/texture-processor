using UnityEngine;

namespace RatKing {
    
    public interface ITextureProcessor {
		Texture2D SourceTexture { get; }
        float Brightness { get; }
        float Contrast { get; }
        float Gamma { get; }
        float Saturation { get; }
        float Red { get; }
        float Green { get; }
        float Blue { get; }
        int Width { get; }
        int Height { get; }
        TextureProcessor.ResizeMethodType ResizeMethod { get; }
    }

}