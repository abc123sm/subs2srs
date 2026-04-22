# 🎬 subs2srs (Custom Fork)

![License](https://img.shields.io/badge/License-GPL%203.0-blue.svg)
![Status](https://img.shields.io/badge/Status-Active-success.svg)

这是一个基于字幕时间轴处理视频或音频，生成 SRS（Spaced Repetition System）软件学习资料的工具，可用于导入 **Anki**、**SuperMemo** 等记忆软件。

原版代码在这里：[subs2srs (SourceForge)](https://subs2srs.sourceforge.net/)。本仓库是在原版基础上的魔改版，主要为了**更符合我个人的使用需求，提升导出效率与现代媒体格式的支持。**

> 具体来说，这玩意能够把视频按照字幕文件的时间轴切成一句一句的独立媒体（音频片段、截图），并生成带有时间戳及对应内容的 `.csv` 文件，可以直接喂给 Anki！

**生成的 CSV 内容示例：**

```csv
BanG_Dream!_01	01_0034_0.04.03.030	[sound:BanG_Dream!_01_0.04.02.930-0.04.04.160.opus]	<img src="BanG_Dream!_01_0.04.03.570.webp">	教室。 あぁ。	教室
BanG_Dream!_01	01_0035_0.04.05.350	[sound:BanG_Dream!_01_0.04.05.250-0.04.09.090.opus]	<img src="BanG_Dream!_01_0.04.07.195.webp">	私　内部生だから 何も変わらないっていうか。	我是直升上来的 所以对我来说没什么区别
BanG_Dream!_01	01_0036_0.04.09.040	[sound:BanG_Dream!_01_0.04.08.940-0.04.12.260.opus]	<img src="BanG_Dream!_01_0.04.10.625.webp">	でも　高校生だよ？ 何か始まる気しない？	但是现在是高中生了啊 不感觉会发生什么事吗
BanG_Dream!_01	01_0037_0.04.12.260	[sound:BanG_Dream!_01_0.04.12.160-0.04.14.270.opus]	<img src="BanG_Dream!_01_0.04.13.240.webp">	え？ もう始まってる。	什么 已经开始了
BanG_Dream!_01	01_0038_0.04.20.570	[sound:BanG_Dream!_01_0.04.20.470-0.04.22.220.opus]	<img src="BanG_Dream!_01_0.04.21.370.webp">	新しい友達。	交到新朋友了
BanG_Dream!_01	01_0039_0.04.23.240	[sound:BanG_Dream!_01_0.04.23.140-0.04.25.270.opus]	<img src="BanG_Dream!_01_0.04.24.230.webp">	友達認定　早いね。 えっ？	你这个朋友认得还真快啊
BanG_Dream!_01	01_0040_0.04.27.210	[sound:BanG_Dream!_01_0.04.27.110-0.04.30.700.opus]	<img src="BanG_Dream!_01_0.04.28.930.webp">	よろしくね　戸山さん。 香澄でいいよ。	请多关照 户山同学 叫我香澄就好啦
```

---

## ✨ 与原版（*29.7版*）的核心区别

### ⚡ 提升导出速度 (多线程并发)
导出时间**快了不少**！但代价是更吃性能。原版是单线程顺序执行，基本可以边导边玩游戏；但我这版基本只能看看视频、聊聊天了（可以在设定中降低线程数以减少性能消耗）。

### 🎵 音频处理优化
- **格式更新**：改成输出体积更小、音质更好的 `opus` 格式音频，并使用 `-application audio` 参数。
- **预留时间**：默认勾选片段时间预留，前后预留默认改为 `100ms`。
- **单声道支持**：新增了一个输出「**单声道**」的按钮！对于学习外语来说立体声其实不是很重要，单声道还能额外省下一半的文件体积（甚至对人声来说效果更好）。

### 🔊 响度均衡
改为直接调用 `rsgain` 进行响度均衡，并且**默认勾选**，保证切分出来的句子音量一致。

### 🖼️ 截图画质增强
改成默认输出 `webp` 格式，预设分辨率改为 `1920 x 1080`。虽然原版你自己在设定里改文件后缀也行，但我这里直接改了预设分辨率，省得每次重装或者新环境都得选半天。

### 🎞️ 视频切割策略
改成可以直接**拷贝流**，且导出格式改为 `mp4`。反正你都已经用视频来学外语了，应该也无所谓视频文件的那点体积吧……  
如果实在在意体积，可以自己先用 `ffmpeg` 压一下原视频，参数调整更自由。

### 🛠️ ffmpeg 版本同步
我会顺手更新 `ffmpeg` 版本（保持与我电脑上的版本相对同步）。虽然可能不会是最新版，但比原版那个停止更新很久的版本新不少！实际感知上可能区别不大，毕竟主要用到转码跟拷贝。

> [!WARNING]
> **关于丢文件的说明**
> 比起原版，这版虽然速度快了，但大概是我代码写得太菜（不懂 C#，AI 写的我稍微改改就下了），并发情况下会偶发丢失一些输出文件。
> 别慌，影响不是很大，**缺失的句子不足 5%**！我认为相比原版那让人昏昏欲睡的导出速度来说是可接受的（至少我个人能接受）。

---

## 🚀 使用方法

### 注意事项与并发设定

> [!CAUTION]
> **切勿抢占 IO 资源**
> 用我这版的时候，**别在导出时弄些对硬盘要求比较高的操作（文件解压、压缩、校验之类的）**！如果要用请务必限速，不然会显著增加丢文件的概率。

> [!TIP]
> **降低线程保平安**
> **线程数拉低一些，可以大幅降低丢文件的概率**！
> 以我的 1370p（大概对应桌面端 i5 13600 输出性能）为例，**使用 5 线程可以做到完全不丢失文件**。开启设定后在 UI 最上方可以看见并发度相关的选项。按需调整即可。

![并发设定](https://github.com/user-attachments/assets/d3079b07-d7a9-4f99-a3e1-040fabe59e91)

### 更多参考资料
- **官方文档**: [https://subs2srs.sourceforge.net/](https://subs2srs.sourceforge.net/)
- **开发者杂谈**: [Telegram Subs2srs频道 (@Subs2srs)](https://t.me/Subs2srs)

---

## 🏎️ 实测速度 (Benchmark)

v5 版实测效率：拿了英配的《摇曳露营》S1~S3 测试，总计 **37 集，8475 句台词**。

**测试环境：**
- **CPU:** 魔改的 MoDT 笔记本 13 代 Intel CPU，尔英 Q1J2 (1370P的工程样品)，功耗墙开到 150w，大概对应 i5 13600
- **内存:** 杂牌 5600 16G x 2
- **硬盘:** 杂牌 PCIe 3.0 M.2（而且是在可用空间不足 5% 的极端情况）

**测试结果：**
- **总耗时:** 20 分钟！
- **导出说明:** 在导出的时候我还在拿电脑跑其他东西，所以纯算力上实际上会更快一点！理论上应该产出 16950 个文件，实际导出刚好就是 16950 个，看起来是一点都没少跑。
- **免责声明:** 至于我这野路子代码有没有隐患，会不会实际上有暗病缺漏或者重复，就不清楚了。如果有的话我再来改 README (。

![实测截图](https://github.com/user-attachments/assets/0ca55469-5839-4894-b952-167cb79f8e26)

*(截图没截到文件总数……反正你们知道没缺文件就行……………………)*

---

## 📞 技术支持 / Support

如遇到问题，请：
1. 先查看原版的 [使用说明 (Documentation)](https://subs2srs.sourceforge.net/)
2. 检查显卡驱动是否为最新版本
3. 提交 Issue 到项目仓库

## ⭐ 小星星 / Star History

[![Star History Chart](https://api.star-history.com/svg?repos=abc123sm/subs2srs&type=Date)](https://star-history.com/#abc123sm/subs2srs&Date)

## 📄 开源协议 (License)
本项目基于原版 subs2srs 进行二次开发，并采用 [GPL-3.0 License](./gpl.txt) 协议开源。原版代码及相关开源组件版权归原作者所有。

## 🙏 鸣谢与致谢 (Acknowledgments)
- **原项目基础:** [subs2srs (SourceForge)](http://sourceforge.net/projects/subs2srs/)，原作者：Christopher Brochtrup (`cb4960@gmail.com`)
- **使用的核心组件与开源库:**
  - `ffmpeg`: 用于视频、音频以及截图的流处理
  - `Subtitle Creator`: 使用了其原始的 VOBSUB 解析代码
  - `TagLib#`: 用于处理 MP3/现代音频格式的标签信息库
  - `rsgain`: 用于音量标准化处理 (ReplayGain)
  - `mkvtoolnix`: 用于从 `.mkv` 视频文件中提取音视频流�但影响不是很大，缺失的句子不足5%，我认为相比原版的导出速度来说是可接受的，至少我个人是可接受  

## 使用方法
用我这版，别在导出时弄些对硬盘要求比较高的操作（文件解压、压缩、校验之类的），如果要用请限速，不然会少文件  
线程数拉低一些，可以降低丢文件的概率，我1370p（大概对应i5 13600）使用5线程可以做到完全不丢失文件  
参考 官方文档 https://subs2srs.sourceforge.net/  
参考 随便写的一些东西 https://t.me/Subs2srs

### 可按需修改并行度，具体看说明
开启设定后最上方可以看见相关选项  
![图片](https://github.com/user-attachments/assets/d3079b07-d7a9-4f99-a3e1-040fabe59e91)

## 实测速度
v5版，拿了英配的摇曳露营S1~S3测试，拢共37集 8475句台词，使用cpu是魔改的modt笔记本的13代intel cpu，尔英q1j2（1370P），功耗墙开到150w，内存杂牌5600 16g*2，硬盘杂牌pcie3.0 m2且可用空间不足5%，导出总时间20分钟  
在导出的时候我还在拿电脑跑其他东西，所以实际上会更快点，理论上出的文件应该是16950个， 实际导出文件16950个，看起来应该是没缺，至于我代码有没有问题，会不会实际上有缺或者重复就不清楚了，如果有的话我再来改readme  

![图片](https://github.com/user-attachments/assets/0ca55469-5839-4894-b952-167cb79f8e26)

截图没截到文件总数……反正你们知道没缺文件就行……………………  

## 📞 技术支持 / Support

如遇到问题，请：
1. 查看[使用说明](https://subs2srs.sourceforge.net/)
2. 加入讨论组后提问 [Telegram Subs2srs频道 (@Subs2srs)](https://t.me/Subs2srs)
3. 提交Issue到项目仓库

## ⭐ 小星星 / Star History

[![Star History Chart](https://api.star-history.com/svg?repos=abc123sm/subs2srs&type=Date)](https://star-history.com/#abc123sm/subs2srs&Date)

## 📄 开源协议 (License)
本项目基于原版 subs2srs 进行二次开发，并采用 [GPL-3.0 License](./gpl.txt) 协议开源。原版代码及相关开源组件版权归原作者所有。

## 鸣谢与致谢 (Acknowledgments)
- **原项目基础:** [subs2srs (SourceForge)](http://sourceforge.net/projects/subs2srs/)，原作者：Christopher Brochtrup (cb4960@gmail.com)
- **使用的核心组件与开源库:**
  - `ffmpeg`: 用于视频、音频以及截图的处理
  - `Subtitle Creator`: 使用了其原始的 VOBSUB 解析代码
  - `TagLib#`: 用于处理 MP3 标签信息的库
  - `rsgain`: 用于音量标准化处理 (ReplayGain)
  - `mkvtoolnix`: 用于从 .mkv 视频文件中提取音视频流
