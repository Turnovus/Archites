If you're reading this, you're probably wondering why the base artifact texture is copy-pasted so
many times in this folder.

RimWorld caches each texture along with its mask in the texture atlas. Every texture on the system
has an entry in the atlas, and that atlas entry can only have a single mask cached alongside it.

The game throws an error if you ever try to use different masks for the same image, and refuses to
load said masks. So this is the only way I can get multiple unique appearences out of the same base
texture.

I could make it so that the pods render in real-time, which would mean that the textures aren't
cached. However, this would lead to longer render times. This solution is the compromise that
I prefer.