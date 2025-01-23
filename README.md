# subs2srs
这是一个基于字幕时间轴处理视频或音频，生成srs软件学习资料的工具，原始代码在这[subs2srs](https://subs2srs.sourceforge.net/)，我自己改了改，代码就不开源了，主要我代码写得有点丑，不好意思发，大概说下与原版的不同之处  


# 与原版（*29.7版*）的区别
## 导出速度
导出时间快了不少，但也比原版吃性能的多，原版如果是稍微现代点的cpu，你可以边导出边做其他事，甚至是玩游戏，我这版基本只能看点视频、聊天了

## 音频：
改成输出 opus格式的音频，并使用 -application voip 参数

## 截图
改成默认输出 webp格式，预设分辨率改为1920 x 1080，其实你原版设定里改文件后缀也行，主要是改了预设分辨率，省的每次都得选  
v4版已经把截图改成并行了

## 视频
改成直接拷贝流，且导出格式改为mp4，你都已经用视频来学东西了，应该也无所谓文件体积吧……  

# 使用方法
参考 官方文档 https://subs2srs.sourceforge.net/  
参考 随便写的一些东西 https://t.me/Subs2srs

# 关于开源
没记错的话我已经把几个主要修改过的地方发出来了，别的基本是跟原版一样，如果说违反开源协议，必须要把所有代码出来的话我再研究研究怎么发，总之请在issues还是啥的告诉我，说实话我自己都记不清自己改了啥，没做过版本控制，直接在vb上搓的，主要改动应该就是目前贴出来这几个，改了核显、copy、并行啥的

# 原版的一些介绍
```
subs2srs Release Notes
--------------------------------------------------------------------------------

What is subs2srs?
-----------------
subs2srs allows you to create import files for Anki or other Spaced Repetition
Systems (SRS) based on your favorite foreign language movies and TV shows to aid
in the language learning process.


How to Install and Launch:
--------------------------
1) Make sure that you have .Net Framework Version 3.5 installed (you probably
   already do). If not, you can get it through Windows update or via the following
   link: http://www.microsoft.com/download/en/details.aspx?id=21

2) Unzip subs2srs.

3) In the unzipped directory, simply double-click subs2srs.exe to launch subs2srs.


Official Project Home:
----------------------
http://sourceforge.net/projects/subs2srs/
(if you didn't download subs2srs here, BEWARE!)


Feedback Thread:
----------------
Post comments/suggestions/bugs/questions here:
http://forum.koohii.com/viewtopic.php?id=2643&p=1


Contact:
--------
Christopher Brochtrup
cb4960@gmail.com


Acknowledgments:
----------------
ffmpeg           - Video/audio/snapshot processing
Subtitle Creator - Original VOBSUB code
TagLib#          - MP3 tagging library
mp3gain          - Used to normalize MP3 files
mkvtoolnix       - Used to extract tracks from .mkv files
```
