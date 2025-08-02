using CommandLine;

namespace DiegoG.DungeonRogue.Data;

public class CLArgs
{
    //dump-level-graphs DEBUG-auto-gen 10
    
    [Option("dump-level-graphs", HelpText = "Dumps all the level graphs that are generated to ./debug/dump/level-graphs")]
    public bool DumpLevelGraphs { get; init; }
    
    [Option("auto-gen-test-levels", Default = 0, HelpText = "Automatically generates the specified amount of levels. To be used in conjunction with 'dump-level-graphs' to quickly dump a lot of level graphs")]
    public int AutoGenTestLevels { get; init; }
}