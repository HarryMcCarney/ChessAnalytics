

let json = System.IO.File.ReadAllText "C:/LichessData/LichessSamples/RatingSample.json"

let deserialized = Json.deserialize<List<VariantRating>> json