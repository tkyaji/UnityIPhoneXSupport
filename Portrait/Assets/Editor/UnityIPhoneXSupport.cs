#if UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class UnityIPhoneXSupport {

    private const float headerSize = 50.0f;
    private const float footerSize = 40.0f;
    private static readonly Color headerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Color footerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private const string headerImage = "header@3x.png";    // Relative path from "Assets/StreamingAssets/"
    private const string footerImage = "footer@3x.png";    // Relative path from "Assets/StreamingAssets/"


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
                               "    if (size.height / size.width > 2.15f) {\n" +
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
                           "    if (rect.size.height / rect.size.width > 2.15f) {{\n" +
                           "        rect.origin.y = {0:0.0#####}f;\n" +
                           "        rect.size.height -= ({0:0.0#####}f + {1:0.0#####}f);\n" +
                           "        _window = [[UIWindow alloc] initWithFrame: rect];\n" +
                           "        UIView *headerView = [[UIView alloc] initWithFrame:CGRectMake(0, -{0:0.0#####}f, rect.size.width, {0:0.0#####}f)];\n" +
                           "        [headerView setBackgroundColor:[UIColor colorWithRed:{2:0.0#####}f green:{3:0.0#####}f blue:{4:0.0#####}f alpha:{5:0.0#####}f]];\n";
        if (!string.IsNullOrEmpty(headerImage)) {
            string headerImageText = "        NSString *headerImagePath = [[NSBundle mainBundle] pathForResource:@\"Data/Raw/{0}\" ofType:nil];\n" +
                                     "        UIImageView *headerImageView = [[UIImageView alloc] initWithImage:[UIImage imageWithContentsOfFile:headerImagePath]];\n" +
                                     "        [headerView addSubview:headerImageView];\n";
            acNewText += string.Format(headerImageText, headerImage);
        }
        acNewText += "        [_window addSubview:headerView];\n" +
                     "        UIView *footerView = [[UIView alloc] initWithFrame:CGRectMake(0, rect.size.height, rect.size.width, {1:0.0#####}f)];\n" +
                     "        [footerView setBackgroundColor:[UIColor colorWithRed:{6:0.0#####}f green:{7:0.0#####}f blue:{8:0.0#####}f alpha:{9:0.0#####}f]];\n";
        if (!string.IsNullOrEmpty(footerImage)) {
            string footerImageText = "        NSString *footerImagePath = [[NSBundle mainBundle] pathForResource:@\"Data/Raw/{0}\" ofType:nil];\n" +
                                     "        UIImageView *footerImageView = [[UIImageView alloc] initWithImage:[UIImage imageWithContentsOfFile:footerImagePath]];\n" +
                                     "        [footerView addSubview:footerImageView];\n";
            acNewText += string.Format(footerImageText, footerImage);
        }
        acNewText += "        [_window addSubview:footerView];\n" +
                     "    }} else {{\n" +
                     "        _window = [[UIWindow alloc] initWithFrame: rect];\n" +
                     "    }}\n";
        acNewText = string.Format(acNewText, headerSize, footerSize,
            headerColor.r, headerColor.g, headerColor.b, headerColor.a,
            footerColor.r, footerColor.g, footerColor.b, footerColor.a);
        appControllerContent = appControllerContent.Replace(acOldText, acNewText);
        File.WriteAllText(appControllerPath, appControllerContent);

        string viewPath = Path.Combine(path, "Classes/UI/UnityView.mm");
        string viewContent = File.ReadAllText(viewPath);
        string vOldText = "    CGRect  frame   = [UIScreen mainScreen].bounds;";
        string vNewText = "    CGRect frame = [UIScreen mainScreen].bounds;\n" +
                          "    if (frame.size.height / frame.size.width > 2.15f) {{\n" +
                          "        frame.size.height -= {0:0.0#####}f;\n" +
                          "    }}";
        vNewText = string.Format(vNewText, headerSize + footerSize);
        viewContent = viewContent.Replace(vOldText, vNewText);
        File.WriteAllText(viewPath, viewContent);
    }
}

#endif
