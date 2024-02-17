namespace ITH;

public static class WeightedList
{
	public static T RandomKey<T>( Dictionary<T, float> weightedDictionary )
	{
		var totalWeight = 0f;

		foreach ( float weight in weightedDictionary.Values )
			totalWeight += weight;

		var randomValue = (float)(new Random().NextDouble() * totalWeight);

		foreach ( KeyValuePair<T, float> entry in weightedDictionary )
		{
			randomValue -= entry.Value;

			if ( randomValue <= 0 )
				return entry.Key;
		}

		return default( T );
	}
}
