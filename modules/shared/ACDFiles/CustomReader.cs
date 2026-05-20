using System;
using System.IO;
using System.Text;

namespace ACDBackend;

/// Author: Danny
///
/// Date: 25/02/2023
///
/// <summary>
/// Reads the bytes within the array and keeps track of position.
/// </summary>
public class CustomReader
{
    private byte[]? data { get; set; }
    private string filePath { get; set; }
    private int position { get; set; } = 0;

    public void prepareReader( string filePath )
    {
        this.filePath = filePath;

        readFile( );

        //They do this in content manager, not sure why, but I might aswell do it too lmao
        if( readInt( ) == -1111 )
            readInt( );
        else
            resetPosition( );
    }

    public void readFile( )
    {
        data = File.ReadAllBytes( filePath );
    }

    public ACDFiles getEntries( )
    {
        ACDFiles entryList = [];

        //Ensure we are not hitting EOF and still trying to read
        while( position < data.Length )
        {
            string entryName = readString( );

            entryList.Add( new ACDFile( )
            {
                fileName = entryName,
                fileData = readEncryptedBytes( ),
                fileType = ACData.ParseType( entryName )
            } );
        }
        cleanUp( );

        return entryList;
    }

    public void cleanUp( )
    {
        resetPosition( );

        data = null;

        filePath = "";
    }

    public void resetPosition( )
    {
        position = 0;
    }

    public string readEncryptedBytes( )
    {
        int length = readInt( );

        byte[] _buffer = new byte[length];

        for( int i = 0; i < length; i++ )
        {
            _buffer[i] = readByte( );
            skip( 3 );
        }

        ACDEncryption.decrypt( _buffer );

        return Encoding.Default.GetString( _buffer );
    }

    public int readInt( )
    {
        return BitConverter.ToInt32( readBytes( 4 ),0 );
    }
    public byte readByte( )
    {
        byte returnData = data[position];
        addPosition( );
        return returnData;
    }
    public byte[] readBytes( int count )
    {
        byte[] _buffer = new byte[count];

        if( BitConverter.IsLittleEndian )
            Array.Reverse( _buffer );

        Array.Copy( data,position,_buffer,0,count );

        addPosition( count );

        return _buffer;
    }
    public string readString( )
    {
        int length = readInt( );

        return Encoding.Default.GetString( readBytes( length ) );
    }

    public void skip( int amount )
    {
        position += amount;
    }

    public void addPosition( int amount = 1 )
    {
        position += amount;
    }
}