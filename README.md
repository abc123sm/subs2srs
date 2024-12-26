# subs2srs
这是一个基于字幕时间轴处理视频或音频，生成srs软件学习资料的工具，原始代码在这[subs2srs](https://subs2srs.sourceforge.net/)，我自己改了改，代码就不开源了，主要我代码写得有点丑，不好意思发，大概说下与原版的不同之处  
如果说违反开源协议，必须要弄代码出来的话我再研究研究怎么发，总之请在issues还是啥的告诉我，我是直接在vb那玩意里面写的，代码里面一堆注释（写了第1 2 3 4 5版代码之类的，懒得弄版本管理全扔注释里面了……）

# 与原版（*29.7版*）的区别
## 导出速度
导出时间基本只有原版1/3不到，快了非常多

## 音频：
改成输出 opus格式的音频，并使用 -application voip 参数

## 截图
改成默认输出 webp格式，预设分辨率改为1920 x 1080

## 视频
改成直接拷贝流，且导出格式改为mp4，你都已经用视频来学东西了，应该也无所谓文件体积吧……  

# 碎碎念
截图目前不太清楚该怎么改好，我改了几版，异步/并行都试过了，导出时间只有原版的1/3还低，但最后都会出问题（少文件），推测是还没剪完计数器就拉满，然后把缓存文件删了，如果音频、截图、视频全都改成异步或者并行速度应该会快更多，目前是顺序一个个跑…………

我不懂c井，不知道该咋解决问题，问了gpt，gpt给我的回答我也看不懂……让gpt帮我改，一样是少文件……我懒得折腾了，不是很想花时间学c井，就这样吧

# 使用方法
参考 官方文档 https://subs2srs.sourceforge.net/  
参考 随便写的一些东西 https://t.me/Subs2srs

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
