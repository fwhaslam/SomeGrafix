// Copyright (c) 2026 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using Scripts.Tools;

using UnityEngine;


public class WorldMesh : MonoBehaviour {

    public GameObject TileTemplate;
    public GameObject SeaTemplate;
    public GameObject WalkTemplate;
    public GameObject MineTemplate;
    public int Size;

    internal Transform TileHolderTFM;
    internal Material MaterialTemplate;


    internal GameObject[,] Tiles;

    // Colors:
    internal readonly Color SEA = new( 0.5f, 0.9f, 1.0f );     // light cyan
    internal readonly Color SWAMP = new( 0.2f, 0.6f, 0.5f );   // dark cyan
    internal readonly Color FOREST = new( 0.3f, 0.8f, 0.3f );  // dark green
    internal readonly Color PLAINS = new( 0.8f, 1f, 0.8f );    // light green
    internal readonly Color DUNES = new( 0.8f, 0.8f, 0.3f );   // sandy yellow
    internal readonly Color HILLS = new( 0.6f, 0.3f, 0.1f );   // dark brown


    // Start is called before the first frame update
    public void Start() {

        MakeViewSettings();

        BuildWorld();
        AddOneRoad();
        AddSomeMines();
    }

    internal void MakeViewSettings(){

        var zmin = 3f;
        var zmax = Mathf.Clamp( Size, zmin, 100f );

        CameraHandler.viewSettingsRequest.Invoke( new ViewSettings(){
            Bounds = new Rect( -Size/2f, -Size/2f, Size, Size ),
            ZMin = zmin,
            ZMax = zmax,
            Tilt = (z) => { return Mathf.Lerp( 30f, 10, (z-zmin)/(zmax-zmin) ); }
        });
    }

    internal void AddOneRoad(){

        var iy = Size/2;
        for (int ix=0;ix<Size;ix++) {

            var tile = Tiles[ix,iy];
            var walk = Instantiate( WalkTemplate, tile.transform );

            walk.transform.localScale = new Vector3( 0.98f, -0.6f, 1f );
            walk.transform.localPosition = new Vector3( 0f, 0f, -0.55f );   // -0.55f = ( cube 1f + walk 0.1f ) / 2
        }

    }

    internal void AddSomeMines(){

        for (int num=0;num<5;num++) {

            while (true) {
                var ix = Random.Range(0,Size);
                var iy = Random.Range(0,Size);
                var tile = Tiles[ix,iy];

                if (tile.transform.localPosition.z<=0f) {
                    var work = Instantiate( MineTemplate, tile.transform );
					work.transform.localPosition = new Vector3( 0f, 0f, -0.50f );   // -0.55f = cube 1f / 2

					var rotate = Random.Range(0,4) * 90f;
					work.transform.localRotation = Quaternion.Euler( 0f, 0f, rotate );
                    break;
                }
            }

        }

    }

    internal void BuildWorld(){
        
        TileHolderTFM = new GameObject("TileHolder").transform;
        MaterialTemplate = TileTemplate.GetComponent<MeshRenderer>().materials[0];

        MakeSea();

        Tiles = new GameObject[Size,Size];

        var mid = Size / 2f;
        for (int ix=0;ix<Size;ix++) {
            for (int iy=0;iy<Size;iy++) {

                var tile = Instantiate( TileTemplate, TileHolderTFM );
                Tiles[ix,iy] = tile;

                tile.name = "Tile:"+ix+"/"+iy;
                tile.transform.localPosition = new Vector3( ix-mid, iy-mid, 0f );
                //tile.transform.localPosition = new Vector3( ix-mid, iy-mid, heights[ix,iy] );

                var tileMTL = Instantiate( MaterialTemplate );
                tile.GetComponent<MeshRenderer>().materials[0] = tileMTL;
                SetColor( tile, PLAINS );
            }
        }

        // swamp
        SplashColor( SWAMP, 0.1f );

        // desert
        SplashColor( DUNES, -0.1f );

        // forest
        SplashColor( FOREST, -0.2f );

        // hills
        SplashColor( HILLS, -0.3f );

        // ocean
        SplashColor( SEA, 0.3f );

        // hide water tiles
        foreach ( var tile in Tiles ) {
            if (tile.transform.localPosition.z==0.3f) {
                tile.SetActive( false );
            }
        }
    }

    /// <summary>
    /// Use 'SeaTemplate' to make wide, flat surface that defines the 'ocean' under and surrounding the island.
    /// </summary>
    internal void MakeSea(){
        var work = Instantiate( SeaTemplate, transform );
        work.transform.localPosition = Vector3.zero;
        work.transform.localScale *= ( Size + 201 );     // NOTE: This QUAD is scale(-1) to flip the texture. 
        SetColor( work, SEA );
    }


    /// <summary>
    /// TODO: replace with a list of predefined materials with colors, and re-use those.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="color"></param>
    internal void SetColor( GameObject tile, Color color ) {
        tile.GetComponent<MeshRenderer>().material.SetColor( "_Color", color );
    }

    internal void SplashColor( Color color, float height ) {

        var cx = Random.Range( 0, Size );
        var cy = Random.Range( 0, Size );

        var radius = Random.Range( (int)(Size/6), (int)(Size/2) );
        var limit = Mathf.Pow( radius, 2f );

//print("CX="+cx+" CY="+cy+" Rad="+radius+" Color="+color);

        for (int ix=cx-radius;ix<=cx+radius;ix++) {
            if (ix<0 || ix>=Size) continue;

            var xdist2 = Mathf.Pow( ix-cx, 2f );

            for (int iy=cy-radius;iy<=cy+radius;iy++){
                if (iy<0 || iy>=Size) continue;

                var dist2 = xdist2 + Mathf.Pow( iy-cy, 2f );
                if (dist2>limit) continue;

                // show more towards center
                if ( Random.Range(0f,1f) < dist2/limit ) continue;

                var tile = Tiles[ix,iy];
                SetColor( tile, color );

                var loc = tile.transform.localPosition;
                tile.transform.localPosition = new Vector3( loc.x, loc.y, height );
            }
        }
    }

    internal float[,] MakeHeights(){

        var work = new float[Size,Size];
        var temp = new float[Size,Size];

        // Add Bumps
        for (int bx=0;bx<5;bx++) {

            AddBump( work );
        }

        WideSmooth( work, temp );

        return work;
    }

    internal void AddBump( float[,] work ){

        var cx = Random.Range( 0, Size );
        var cy = Random.Range( 0, Size );

        var radius = Random.Range( (int)(Size/6), (int)(Size/2) );
        var limit = Mathf.Pow( radius, 2f );

//print("CX="+cx+" CY="+cy+" Rad="+radius+" Color="+color);

        for (int ix=cx-radius;ix<=cx+radius;ix++) {
            if (ix<0 || ix>=Size) continue;

            var xdist2 = Mathf.Pow( ix-cx, 2f );

            for (int iy=cy-radius;iy<=cy+radius;iy++){
                if (iy<0 || iy>=Size) continue;

                var dist2 = xdist2 + Mathf.Pow( iy-cy, 2f );
                if (dist2>limit) continue;

                // bigger towards center, remember negative Z is closer to camera
                work[ix,iy] = - ( 1f - Mathf.Sqrt( dist2 ) / radius );

            }
        }
    }


    static readonly int SMOOTH_FRAME = 1;

    internal void WideSmooth( float[,] work, float[,] temp ) {

        for (int ix=0;ix<Size;ix++) {
            var left = Mathf.Max( ix-SMOOTH_FRAME, 0 );
            var right = Mathf.Min( ix+SMOOTH_FRAME, Size-1 );

            for (int iy=0;iy<Size;iy++) {
                var top = Mathf.Max( iy-SMOOTH_FRAME, 0 );
                var bot = Mathf.Min( iy+SMOOTH_FRAME, Size-1 );

                // find sum of cells
                var sum = 0f;
                for ( int lix=left;lix<=right;lix++) {
                    for (int liy=top;liy<=bot;liy++) {
                        sum += work[lix,liy];
                    }
                }

                // average of cells
                var num = ( right-left+1 ) * (bot-top+1);
                var avg = sum / num;

                // only smooth extremes
                var old = work[ix,iy];
                temp[ix,iy] = ( Mathf.Abs( old-avg ) > 0.25f ? avg : old );
            }
         }

        // copy back into original array
        System.Array.Copy( temp, work, work.Length );
    }
}
