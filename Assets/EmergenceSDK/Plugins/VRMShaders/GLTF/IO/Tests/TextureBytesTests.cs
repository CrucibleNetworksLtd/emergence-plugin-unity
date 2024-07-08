using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public class TextureBytesTests
    {

        [Test]
        public void NonReadablePng()
        {
            var nonReadableTex = Resources.Load("4x4_non_readable") as Texture2D;
            Assert.False(nonReadableTex.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(nonReadableTex, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }

        [Test]
        public void NonReadableDds()
        {
            var readonlyTexture = Resources.Load("4x4_non_readable_compressed") as Texture2D;
            Assert.False(readonlyTexture.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(readonlyTexture, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }
    }
}
