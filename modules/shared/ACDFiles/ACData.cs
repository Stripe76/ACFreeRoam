using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Framework.IniFiles;

namespace ACDBackend;

public class ACData
{
    private readonly CustomReader reader = new();
    private readonly CustomWriter writer = new();

    public string dataFilePrefix { get; set; } = "data.acd";
    public string dataFolderPrefix { get; set; } = "data";

    public ACDFiles GetEntries( string carFolder )
    {
        //If the data folder already exists, just read the files from the folder.
        string dataFolder = Path.Combine( carFolder,dataFolderPrefix );
        if( Directory.Exists( dataFolder ) )
        {
            ACDFiles entries = [];

            foreach( FileInfo file in new DirectoryInfo( dataFolder ).GetFiles( ) )
            {
                entries.Add( new ACDFile( )
                {
                    fileName = file.Name,
                    fileData = File.ReadAllText( file.FullName ),
                    fileType = ParseType( file.Name )
                } );
            }
            return entries;
        }
        else
        {
            string dataFile = Path.Combine( carFolder,dataFilePrefix );
            if( File.Exists( dataFile ) )
            {
                SetupEncryption( carFolder );

                reader.prepareReader( dataFile );

                return reader.getEntries( );
            }
            else
            {
                GD.PushError( $"{carFolder} contains no ACD or Data folder, skipping..." );

                return [];
            }
        }
    }

    public static FileTypes ParseType(string name)
    {
        FileTypes returnType = FileTypes.OTHER;
        try
        {
            //Get past the dot.
            returnType = (FileTypes)Enum.Parse( typeof(FileTypes),name.Trim( ).Split( '.' )[1],true );
        }
        catch( Exception )
        {
            // ignored
        }
        //If it shits itself, just return OTHER.

        return returnType;
    }

    private static void SetupEncryption(string filePath)
    {
        string folderName = GetFolderName(filePath);

        //Setup encryption, using folder name as encryption key.
        ACDEncryption.setupEncryption( folderName );
    }

    private static string GetFolderName(string acdFilename)
    {
        var name = Path.GetFileName(acdFilename) ?? "";
        return name.StartsWith( "data",StringComparison.OrdinalIgnoreCase ) ? Path.GetFileName( Path.GetDirectoryName( acdFilename ) ) : name;
    }
}

public class ACDFiles : List<ACDFile>
{
    public ACDFile? GetFile( string fileName )
    {
        return this.FirstOrDefault( file => file.fileName.Equals( fileName,StringComparison.InvariantCultureIgnoreCase ) );
    }
}

public class ACDFile
{
    private IniFile? _iniFile = null;
    
    public FileTypes fileType { get; set; }

    public string fileName { get; set; }
    public string fileData { get; set; }

    public IniFile? IniFile
    {
        get
        {
            _iniFile ??= LoadIniFile( );

            return _iniFile;
        }
    }

    private IniFile? LoadIniFile( )
    {
        return new IniFile( fileData,true );
    }
}

public enum FileTypes
{
    INI,
    LUT,
    OTHER
}

