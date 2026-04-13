日常提交更改（本机 → GitHub）
git add -A
git commit -m "update"
git push


（如果这是新加的大文件类型，先执行一次）

git lfs track "*.xxx"
git add .gitattributes
git commit -m "update lfs rules"
git push

在另一台电脑 / 拉取最新更改（GitHub → 本机）

首次获取项目：

git clone <仓库地址>
cd <仓库目录>
git lfs install
git lfs pull


以后同步最新版本：

git pull
git lfs pull

常用保险命令

查看 LFS 文件状态：

git lfs ls-files


发现贴图/模型丢失时强制重拉：

git lfs pull