namespace BrickJam.Upgrading;

public static class Creator
{
	static Creator() => Build();

	[Event.Hotload]
	internal static void Build()
	{
		Upgrade.ClearAll();

		// Example
		new Upgrade.Builder( "Jellyfish Jam", "Applies jellyfish jam on your body. Makes you faster." )
			.ConfigureWith( v =>
				v.SpeedMultiplier = 0.05f )
			.WithPrice( 100 )
			.Build();

		
		/*new Upgrade.Builder( "Aura of Fear I", "Enemies around you have lower diligence." )
			.ConfigureWith( v =>
			{
				v.AuraOfFear = 0.05f; // -5%
			} )
			.WithTexture( "fear/fear1.png" )
			.Next( "Aura of Fear II",
				v =>
					v.WithTexture( "fear/fear2.png" )
						.PlaceAt( Vector2.Left * 150 ) )
			.Next( "Aura of Fear III",
				v =>
					v.WithTexture( "fear/fear3.png" )
						.PlaceAt( Vector2.Left * 300 ) )
			.Next( "Aura of Fear IV",
				v =>
					v.WithTexture( "fear/fear4.png" )
						.PlaceAt( Vector2.Left * 450 ) )
			.Next( "Aura of Fear V",
				v =>
					v.WithTexture( "fear/fear5.png" )
						.PlaceAt( Vector2.Left * 600 ) )
			.Build();*/
	}
}
