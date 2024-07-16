namespace MauiBlazor.Models
{
    public static class ScannerType
    {
        public const int TWAINSCANNER = 0x10;
        public const int WIASCANNER = 0x20;
        public const int TWAINX64SCANNER = 0x40;
        public const int ICASCANNER = 0x80;
        public const int SANESCANNER = 0x100;
        public const int ESCLSCANNER = 0x200;
        public const int WIFIDIRECTSCANNER = 0x400;
        public const int WIATWAINSCANNER = 0x800;
    }
}
