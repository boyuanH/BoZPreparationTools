using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoZPreparation_Tool
{
    public class BoZConstant
    {
        public static string nStartFrameNo = "1";
        public static string ProcessingFrameNum = "1200";
        public static string CameraSettingSegment = @"CameraSetting";

        public static string DefaultCameraSettingFileName = "CameraSetting.ini";
        public static string BirdViewConfigFile = @"BirdsView.ini";
        public static string LRU3DConfigFile = @"Param_3DPos.ini";
        public static string BirdViewConfigPara_CameraHeight        = @"CameraHeight ";
        public static string BirdViewConfigPara_CameraPitch         = @"CameraPitch ";
        public static string BirdViewConfigPara_ProcessingFrameNum  = @"ProcessingFrameNum";
        public static string BirdViewConfigPara_StartFrameNo        = @"StartFrameNo";
        public static string BirdViewConfigPara_ObjInfo             = @"ObjInfo";
        public static string BirdViewConfigPara_SaveDataPath        = @"SaveDataPath";
        public static string BirdViewConfigPara_FrameSkipNum        = @"FrameSkipNum";

        public static string LRU3DConfigPara_CameraHeight = @"CameraHeight ";
        public static string LRU3DConfigPara_CameraPitch = @"CameraPitch ";
        public static string LRU3DConfigPara_ProcessingFrameNum = @"ProcessingFrameNum";
        public static string LRU3DConfigPara_StartFrameNo = @"StartFrameNo";
        public static string LRU3DConfigPara_OutCsvFilename = @"OutCsvFilename";
        public static string LRU3DConfigPara_LearningResultDir = @"LearningResultDir";
        public static string LRU3DConfigPara_RawImageDataDir = @"RawImageDataDir";
        public static string LRU3DConfigPara_SaveDataPath = @"SaveDataPath";

        public static string CopyResultsExe = "CopyResults.exe";

    }
}
