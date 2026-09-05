using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Villsource.ObjectValue;

public readonly partial record struct Address()
{
    public string? RawAddress { get; init; } // เก็บข้อความต้นฉบับ
    public string? HouseNo { get; init; } // บ้านเลขที่
    public string? Building { get; init; } // หมู่บ้าน / ชื่ออาคาร / ชั้น
    public string? Moo { get; init; } // หมู่
    public string? Soi { get; init; } // ซอย
    public string? Road { get; init; } // ถนน
    public string? SubDistrict { get; init; } // ตำบล / แขวง
    public string? District { get; init; } // อำเภอ / เขต
    public string? Province { get; init; } // จังหวัด
    public string? PostalCode { get; init; } // รหัสไปรษณีย์
    public string? Country { get; init; } // ประเทศ

    public Address(
        string? houseNo, string? subDistrict, string? district,
        string? province, string? postalCode, string? country = "Thailand",
        string? building = null, string? moo = null, string? soi = null, string? road = null) : this()
    {
        HouseNo = houseNo;
        Building = building;
        Moo = moo;
        Soi = soi;
        Road = road;
        SubDistrict = subDistrict;
        District = district;
        Province = province;
        PostalCode = postalCode;
        Country = country ?? "Thailand";
    }

    public Address(string rawAddress) : this()
    {
        RawAddress = rawAddress.NullIfEmpty();
    }

    public static Address Empty() => new();

    public override string ToString()
    {
        // หากไม่มีข้อมูลถูกแยกออกมาเลย แต่มี RawAddress ให้คืนค่า RawAddress กลับไป
        if (string.IsNullOrWhiteSpace(Province) &&
            string.IsNullOrWhiteSpace(HouseNo) &&
            !string.IsNullOrWhiteSpace(RawAddress))
        {
            return RawAddress;
        }

        var sb = new StringBuilder(128);

        if (!string.IsNullOrWhiteSpace(HouseNo))
            sb.Append(HouseNo).Append(' ');

        if (!string.IsNullOrWhiteSpace(Moo))
            sb.Append("หมู่ ").Append(Moo).Append(' ');

        if (!string.IsNullOrWhiteSpace(Building))
            sb.Append(Building).Append(' ');

        if (!string.IsNullOrWhiteSpace(Soi))
            sb.Append("ซอย").Append(Soi).Append(' ');

        if (!string.IsNullOrWhiteSpace(Road))
            sb.Append("ถนน").Append(Road).Append(' ');

        bool isBkk = Province?.Contains("กรุงเทพ", StringComparison.Ordinal) == true ||
                     Province?.Contains("Bangkok", StringComparison.Ordinal) == true;

        if (!string.IsNullOrWhiteSpace(SubDistrict))
        {
            sb.Append(isBkk ? "แขวง" : "ต.");
            sb.Append(SubDistrict).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(District))
        {
            sb.Append(isBkk ? "เขต" : "อ.");
            sb.Append(District).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(Province))
        {
            if (!isBkk) sb.Append("จ.");
            sb.Append(Province).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(PostalCode))
            sb.Append(PostalCode).Append(' ');

        if (!string.IsNullOrWhiteSpace(Country) && !Country.Equals("Thailand", StringComparison.OrdinalIgnoreCase))
            sb.Append(Country);

        // คืนค่าพร้อมลบช่องว่างส่วนเกินที่อาจเกิดจากการต่อ String
        return sb.ToString().TrimEnd();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public static bool TryParse(string input, out Address? address)
    {
        address = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            address = Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Address Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Address string cannot be empty", nameof(input));

        string rawInput = input; // เก็บต้นฉบับไว้ก่อน

        // คลีนช่องว่างที่ติดกันหลายๆ ตัว
        input = MultipleSpaceRegex().Replace(input, " ").Trim();

        var provinceMatch = ProvinceRegex().Match(input);
        string? province = provinceMatch.Success ? provinceMatch.Groups[1].Value : null;

        if (string.IsNullOrEmpty(province) &&
            (input.Contains("กรุงเทพ", StringComparison.Ordinal) ||
             input.Contains("กทม", StringComparison.Ordinal)))
        {
            province = "กรุงเทพมหานคร";
        }

        // ใช้ Object Initializer ทำให้โค้ดอ่านง่ายขึ้นและเร็วกว่าการส่งผ่าน Parameter ยาวๆ
        return new Address
        {
            RawAddress = rawInput, // เก็บค่าดั้งเดิมไว้
            Country = "Thailand",
            PostalCode = PostRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            HouseNo = HomeNoRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            Moo = MooRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            Building = BuildingRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            Soi = SoiRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            Road = RoadRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            SubDistrict = SubDistrictRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            District = DistrictRegex().Match(input).Groups[1].Value.NullIfEmpty(),
            Province = province
        };
    }

    // ----------------------------------------------------
    // Regex Optimizations (เติม CultureInvariant เพิ่มความเร็ว)
    // ----------------------------------------------------

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpaceRegex();

    [GeneratedRegex(@"\b(\d{5})\b", RegexOptions.CultureInvariant)]
    private static partial Regex PostRegex();

    [GeneratedRegex(@"(?:บ้านเลขที่|เลขที่)?\s*(\d+[a-zA-Z\d]*[/\d]*)", RegexOptions.CultureInvariant)]
    private static partial Regex HomeNoRegex();

    [GeneratedRegex(@"(?:หมู่ที่|หมู่|ม\.)\s*(\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex MooRegex();

    [GeneratedRegex(@"(?:หมู่บ้าน|อาคาร|ตึก|คอนโด)\s*([a-zA-Zก-๙0-9]+(?:\s[a-zA-Zก-๙0-9]+)*)",
        RegexOptions.CultureInvariant)]
    private static partial Regex BuildingRegex();

    [GeneratedRegex(@"(?:ซอย|ซ\.)\s*([a-zA-Zก-๙0-9]+(?:\s[a-zA-Zก-๙0-9]+)*)", RegexOptions.CultureInvariant)]
    private static partial Regex SoiRegex();

    [GeneratedRegex(@"(?:ถนน|ถ\.)\s*([a-zA-Zก-๙0-9]+)", RegexOptions.CultureInvariant)]
    private static partial Regex RoadRegex();

    [GeneratedRegex(@"(?:ตำบล|ต\.|แขวง)\s*([a-zA-Zก-๙]+)", RegexOptions.CultureInvariant)]
    private static partial Regex SubDistrictRegex();

    [GeneratedRegex(@"(?:อำเภอ|อ\.|เขต)\s*([a-zA-Zก-๙]+)", RegexOptions.CultureInvariant)]
    private static partial Regex DistrictRegex();

    [GeneratedRegex(@"(?:จังหวัด|จ\.)\s*([a-zA-Zก-๙]+)", RegexOptions.CultureInvariant)]
    private static partial Regex ProvinceRegex();
}

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

public static class AddressConfigurationExtensions
{
    /// <summary>
    /// Helper สำหรับตั้งค่า Complex Type ของ Address พร้อมกำหนด Prefix ของชื่อ Column ใน Database
    /// </summary>
    public static ComplexPropertyBuilder<Address> ConfigureAddress(
        this ComplexPropertyBuilder<Address> builder, 
        string columnPrefix = "Address", 
        bool isRequired = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.IsRequired(isRequired);

        builder.Property(a => a.HouseNo).HasMaxLength(50).HasColumnName($"{columnPrefix}_HouseNo");
        builder.Property(a => a.Building).HasMaxLength(100).HasColumnName($"{columnPrefix}_Building");
        builder.Property(a => a.Moo).HasMaxLength(10).HasColumnName($"{columnPrefix}_Moo");
        builder.Property(a => a.Soi).HasMaxLength(100).HasColumnName($"{columnPrefix}_Soi");
        builder.Property(a => a.Road).HasMaxLength(100).HasColumnName($"{columnPrefix}_Road");
        builder.Property(a => a.SubDistrict).HasMaxLength(100).HasColumnName($"{columnPrefix}_SubDistrict");
        builder.Property(a => a.District).HasMaxLength(100).HasColumnName($"{columnPrefix}_District");
        builder.Property(a => a.Province).HasMaxLength(100).HasColumnName($"{columnPrefix}_Province");
        builder.Property(a => a.PostalCode).HasMaxLength(10).HasColumnName($"{columnPrefix}_PostalCode");
        builder.Property(a => a.Country).HasMaxLength(50).HasColumnName($"{columnPrefix}_Country");
        builder.Property(a => a.RawAddress).HasMaxLength(500).HasColumnName($"{columnPrefix}_RawAddress");

        return builder;
    }
}