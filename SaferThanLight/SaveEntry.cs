using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using z16.Core;

namespace SaferThanLight;

public class SaveEntry : INotifyPropertyChanged {
	public static async Task<SaveEntry> Create() {
		await using var file = File.OpenRead(Data.SaveFile);
		using var reader = new BinaryReader(file, Encoding.UTF8, true);

		reader.Advance(8);
		var advancedValue = reader.ReadInt32();
		var difficultyValue = reader.ReadInt32();
		reader.Advance(16);
		var shipName = reader.ReadUtf8PrefixString();
		var shipId = reader.ReadUtf8PrefixString();
		var hasLayoutSuffix = shipId[^2] == '_' && Char.IsDigit(shipId[^1]);

		var now = DateTime.UtcNow;
		return new SaveEntry() {
			Date = now,
			Filename = $"continue_{now.ToFileTime()}",
			Advanced = advancedValue != 0,
			Difficulty = (Difficulty) difficultyValue,
			Type = TypeMap[hasLayoutSuffix ? shipId[0..^2] : shipId],
			Layout = hasLayoutSuffix ? (ShipLayout) Int32.Parse(shipId[^1..^0]) : ShipLayout.A,
			Sector = reader.ReadInt32(),
		};
	}

	public DateTime Date { get; set; }
	public String Filename { get; set; } = null!;
	public Boolean Advanced { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ShipType Type { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ShipLayout Layout { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public Difficulty Difficulty { get; set; }
	public Int32 Sector { get; set; }

	public String Description {
		get { return DescriptionField; }
		set {
			DescriptionField = value;
			NotifyPropertyChanged();
		}
	}

	internal String Filepath => Path.Combine(Data.SaveDirectory, Filename);

	private String DescriptionField = "";

	private static readonly IReadOnlyDictionary<String, ShipType> TypeMap = new Dictionary<String, ShipType>() {
		["PLAYER_SHIP_HARD"] = ShipType.Kestrel,
		["PLAYER_SHIP_MANTIS"] = ShipType.Mantis,
		["PLAYER_SHIP_STEALTH"] = ShipType.Stealth,
		["PLAYER_SHIP_CIRCLE"] = ShipType.Engi,
		["PLAYER_SHIP_FED"] = ShipType.Federation,
		["PLAYER_SHIP_JELLY"] = ShipType.Slug,
		["PLAYER_SHIP_ROCK"] = ShipType.Rock,
		["PLAYER_SHIP_ENERGY"] = ShipType.Zoltan,
		["PLAYER_SHIP_CRYSTAL"] = ShipType.Crystal,
		["PLAYER_SHIP_ANAEROBIC"] = ShipType.Lanius,
	};

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void NotifyPropertyChanged([CallerMemberName] String? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
