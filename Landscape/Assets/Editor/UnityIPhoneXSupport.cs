#if UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class UnityIPhoneXSupport {

    private const float leftSize = 45.0f;
    private const float rightSize = 45.0f;
    private static readonly Color leftColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Color rightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private const string leftImage = "left@3x.png";    // Relative path from "Assets/StreamingAssets/"
    private const string rightImage = "right@3x.png";    // Relative path from "Assets/StreamingAssets/"


    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path) {
        if (UnityEditor.PlayerSettings.statusBarHidden) {
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetBoolean("UIStatusBarHidden", false);
            plist.WriteToFile(plistPath);

            string viewControllerPath = Path.Combine(path, "Classes/UI/UnityViewControllerBaseiOS.mm");
            if (!File.Exists(viewControllerPath)) {
                viewControllerPath = Path.Combine(path, "Classes/UI/UnityViewControllerBase+iOS.mm");
            }
            string viewControllerContent = File.ReadAllText(viewControllerPath);
            string vcOldText = "    return _PrefersStatusBarHidden;";
            string vcNewText = "    CGSize size = [UIScreen mainScreen].bounds.size;\n" +
                               "    if (size.width / size.height > 2.15f) {\n" +
                               "        return NO;\n" +
                               "    } else {\n" +
                               "        return YES;\n" +
                               "    }";
            viewControllerContent = viewControllerContent.Replace(vcOldText, vcNewText);
            File.WriteAllText(viewControllerPath, viewControllerContent);
        }

        string appControllerPath = Path.Combine(path, "Classes/UnityAppController.mm");
        string appControllerContent = File.ReadAllText(appControllerPath);
        string acOldText = "    _window         = [[UIWindow alloc] initWithFrame: [UIScreen mainScreen].bounds];";
        string acNewText = "    CGRect rect = [UIScreen mainScreen].bounds;\n" +
                           "    if (rect.size.width / rect.size.height > 2.15f) {{\n" +
                           "        rect.origin.x = {0:0.0#####}f;\n" +
                           "        rect.size.width -= ({0:0.0#####}f + {1:0.0#####}f);\n" +
                           "        _window = [[UIWindow alloc] initWithFrame: rect];\n" +
                           "        UIView *leftView = [[UIView alloc] initWithFrame:CGRectMake(-{0:0.0#####}f, 0, {0:0.0#####}f, rect.size.height)];\n" +
                           "        [leftView setBackgroundColor:[UIColor colorWithRed:{2:0.0#####}f green:{3:0.0#####}f blue:{4:0.0#####}f alpha:{5:0.0#####}f]];\n";
        if (!string.IsNullOrEmpty(leftImage)) {
            string leftImageText = "        NSString *leftImagePath = [[NSBundle mainBundle] pathForResource:@\"Data/Raw/{0}\" ofType:nil];\n" +
                                   "        UIImageView *leftImageView = [[UIImageView alloc] initWithImage:[UIImage imageWithContentsOfFile:leftImagePath]];\n" +
                                   "        [leftView addSubview:leftImageView];\n";
            acNewText += string.Format(leftImageText, leftImage);
        }
        acNewText += "        [_window addSubview:leftView];\n" +
                     "        UIView *rightView = [[UIView alloc] initWithFrame:CGRectMake(rect.size.width, 0, rect.size.width, {1:0.0#####}f)];\n" +
                     "        [rightView setBackgroundColor:[UIColor colorWithRed:{6:0.0#####}f green:{7:0.0#####}f blue:{8:0.0#####}f alpha:{9:0.0#####}f]];\n";
        if (!string.IsNullOrEmpty(rightImage)) {
            string rightImageText = "        NSString *rightImagePath = [[NSBundle mainBundle] pathForResource:@\"Data/Raw/{0}\" ofType:nil];\n" +
                                    "        UIImageView *rightImageView = [[UIImageView alloc] initWithImage:[UIImage imageWithContentsOfFile:rightImagePath]];\n" +
                                    "        [rightView addSubview:rightImageView];\n";
            acNewText += string.Format(rightImageText, rightImage);
        }
        acNewText += "        [_window addSubview:rightView];\n" +
                     "    }} else {{\n" +
                     "        _window = [[UIWindow alloc] initWithFrame: rect];\n" +
                     "    }}\n";
        acNewText = string.Format(acNewText, leftSize, rightSize,
            leftColor.r, leftColor.g, leftColor.b, leftColor.a,
            rightColor.r, rightColor.g, rightColor.b, rightColor.a);
        appControllerContent = appControllerContent.Replace(acOldText, acNewText);
        File.WriteAllText(appControllerPath, appControllerContent);

        string viewPath = Path.Combine(path, "Classes/UI/UnityView.mm");
        string viewContent = File.ReadAllText(viewPath);
        string vOldText = "    CGRect  frame   = [UIScreen mainScreen].bounds;";
        string vNewText = "    CGRect frame = [UIScreen mainScreen].bounds;\n" +
                          "    if (frame.size.width / frame.size.height > 2.15f) {{\n" +
                          "        frame.size.width -= {0:0.0#####}f;\n" +
                          "    }}";
        vNewText = string.Format(vNewText, leftSize + rightSize);
        viewContent = viewContent.Replace(vOldText, vNewText);
        File.WriteAllText(viewPath, viewContent);
    }
}

#endif
