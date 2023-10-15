namespace BrickJam;

public partial class Item : UseableEntity
{
	public ItemPrefab Prefab;

	[Net]
	public ItemRarity Rarity { get; set; }

	[Net]
	public int MonetaryValue { get; set; }

	public static Item CreateFromGameResource( ItemPrefab resource, Vector3 position, Rotation rotation )
	{
		var item = new Item();

		item.Tags.Add( "nocollide" );
		item.Position = position;
		item.Rotation = rotation;
		item.Rarity = resource.Rarity;
		item.MonetaryValue = resource.MonetaryValue;
		item.SetModel( resource.Model == string.Empty ? "models/error.vmdl" : resource.Model );
		item.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		// TODO: Might not need to attach the ItemPrefab member at all. 
		// See if there's any reason to include it on the Item entity.
		item.Prefab = resource;
		
		return item;
	}

	public override void Use( Player user )
	{
		base.Use( user );

		user.AddMoney( MonetaryValue );
		Delete();
	}
}
