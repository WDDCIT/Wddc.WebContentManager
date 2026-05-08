namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public string MobileFilename { get; set; }
        public bool HasMobileVersion { get; set; }
        public bool Active { get; set; }
        public bool IsNew { get; set; }
    }

    public class BannerCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BannerCategoryMappingDto
    {
        public int Id { get; set; }
        public int BannerId { get; set; }
        public int CategoryId { get; set; }
    }

    public class BannersOfCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Filename { get; set; }
        public string MobileFilename { get; set; }
        public bool HasMobileVersion { get; set; }
        public bool Active { get; set; }
        public bool IsNew { get; set; }
    }

    public class MediaSettingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
