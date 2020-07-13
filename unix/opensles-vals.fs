\ OpenSLES values

\c #include <pthread.h>
\c typedef struct {
\c    Cell ** queue;
\c    pthread_mutex_t *lock;
\c    FILE * wakeup;
\c    SLPlayItf player;
\c } sl_queue;
\c typedef struct {
\c        Char litx;
\c        Char lit[sizeof(Cell)];
\c        Char wakex;
\c    } wakeup;
\c void simple_buffer_cb(SLBufferQueueItf caller, sl_queue* pContext) {
\c    Cell size;
\c    static wakeup wk = { 1, "", 3 };
\c    pthread_mutex_lock(pContext->lock);
\c    if(pContext->queue) {
\c      Cell * buffer=0;
\c      while((size=(Cell)(pContext->queue[0])) > 0) {
\c        Cell * buffer1=pContext->queue[1];
\c        pContext->queue[0]=(Cell*)(size-=sizeof(Cell));
\c        memmove(pContext->queue+1, pContext->queue+2, size);
\c        if(buffer) {
\c          buffer=realloc(buffer, buffer[0]+buffer1[0]+sizeof(Cell));
\c          memmove((Char*)(buffer+1)+buffer[0], (Char*)(buffer1+1), buffer1[0]);
\c          buffer[0]+=buffer1[0];
\c          free(buffer1);
\c        } else {
\c          buffer = buffer1;
\c        }
\c      }
\c      if(buffer && buffer[0]) {
\c        const struct SLBufferQueueItf_* ptr = *caller;
\c        ptr->Enqueue(caller, buffer+1, buffer[0]);
\c        free(buffer);
\c      } else {
\c        const struct SLPlayItf_ *ptr = *(pContext->player);
\c        ptr->SetPlayState(pContext->player, 2);
\c      }
\c      free(pContext->queue);
\c      pContext->queue=0;
\c      fwrite((void*)&wk, 1, sizeof(wk), pContext->wakeup); 
\c    }
\c    pthread_mutex_unlock(pContext->lock);
\c }

c-value simple-buffer-cb &simple_buffer_cb -- a

c-value SL_IID_ANDROIDEFFECT SL_IID_ANDROIDEFFECT -- a
c-value SL_IID_ANDROIDEFFECTSEND SL_IID_ANDROIDEFFECTSEND -- a
c-value SL_IID_ANDROIDEFFECTCAPABILITIES SL_IID_ANDROIDEFFECTCAPABILITIES -- a
c-value SL_IID_ANDROIDCONFIGURATION SL_IID_ANDROIDCONFIGURATION -- a
c-value SL_IID_ANDROIDSIMPLEBUFFERQUEUE SL_IID_ANDROIDSIMPLEBUFFERQUEUE -- a
c-value SL_IID_ANDROIDBUFFERQUEUESOURCE SL_IID_ANDROIDBUFFERQUEUESOURCE -- a
c-value SL_IID_ANDROIDACOUSTICECHOCANCELLATION SL_IID_ANDROIDACOUSTICECHOCANCELLATION -- a
c-value SL_IID_ANDROIDAUTOMATICGAINCONTROL SL_IID_ANDROIDAUTOMATICGAINCONTROL -- a
c-value SL_IID_ANDROIDNOISESUPPRESSION SL_IID_ANDROIDNOISESUPPRESSION -- a
c-value SL_IID_NULL SL_IID_NULL -- a
c-value SL_IID_OBJECT SL_IID_OBJECT -- a
c-value SL_IID_AUDIOIODEVICECAPABILITIES SL_IID_AUDIOIODEVICECAPABILITIES -- a
c-value SL_IID_LED SL_IID_LED -- a
c-value SL_IID_VIBRA SL_IID_VIBRA -- a
c-value SL_IID_METADATAEXTRACTION SL_IID_METADATAEXTRACTION -- a
c-value SL_IID_METADATATRAVERSAL SL_IID_METADATATRAVERSAL -- a
c-value SL_IID_DYNAMICSOURCE SL_IID_DYNAMICSOURCE -- a
c-value SL_IID_OUTPUTMIX SL_IID_OUTPUTMIX -- a
c-value SL_IID_PLAY SL_IID_PLAY -- a
c-value SL_IID_PREFETCHSTATUS SL_IID_PREFETCHSTATUS -- a
c-value SL_IID_PLAYBACKRATE SL_IID_PLAYBACKRATE -- a
c-value SL_IID_SEEK SL_IID_SEEK -- a
c-value SL_IID_RECORD SL_IID_RECORD -- a
c-value SL_IID_EQUALIZER SL_IID_EQUALIZER -- a
c-value SL_IID_VOLUME SL_IID_VOLUME -- a
c-value SL_IID_DEVICEVOLUME SL_IID_DEVICEVOLUME -- a
c-value SL_IID_BUFFERQUEUE SL_IID_BUFFERQUEUE -- a
c-value SL_IID_PRESETREVERB SL_IID_PRESETREVERB -- a
c-value SL_IID_ENVIRONMENTALREVERB SL_IID_ENVIRONMENTALREVERB -- a
c-value SL_IID_EFFECTSEND SL_IID_EFFECTSEND -- a
c-value SL_IID_3DGROUPING SL_IID_3DGROUPING -- a
c-value SL_IID_3DCOMMIT SL_IID_3DCOMMIT -- a
c-value SL_IID_3DLOCATION SL_IID_3DLOCATION -- a
c-value SL_IID_3DDOPPLER SL_IID_3DDOPPLER -- a
c-value SL_IID_3DSOURCE SL_IID_3DSOURCE -- a
c-value SL_IID_3DMACROSCOPIC SL_IID_3DMACROSCOPIC -- a
c-value SL_IID_MUTESOLO SL_IID_MUTESOLO -- a
c-value SL_IID_DYNAMICINTERFACEMANAGEMENT SL_IID_DYNAMICINTERFACEMANAGEMENT -- a
c-value SL_IID_MIDIMESSAGE SL_IID_MIDIMESSAGE -- a
c-value SL_IID_MIDIMUTESOLO SL_IID_MIDIMUTESOLO -- a
c-value SL_IID_MIDITEMPO SL_IID_MIDITEMPO -- a
c-value SL_IID_MIDITIME SL_IID_MIDITIME -- a
c-value SL_IID_AUDIODECODERCAPABILITIES SL_IID_AUDIODECODERCAPABILITIES -- a
c-value SL_IID_AUDIOENCODERCAPABILITIES SL_IID_AUDIOENCODERCAPABILITIES -- a
c-value SL_IID_AUDIOENCODER SL_IID_AUDIOENCODER -- a
c-value SL_IID_BASSBOOST SL_IID_BASSBOOST -- a
c-value SL_IID_PITCH SL_IID_PITCH -- a
c-value SL_IID_RATEPITCH SL_IID_RATEPITCH -- a
c-value SL_IID_VIRTUALIZER SL_IID_VIRTUALIZER -- a
c-value SL_IID_VISUALIZATION SL_IID_VISUALIZATION -- a
c-value SL_IID_ENGINE SL_IID_ENGINE -- a
c-value SL_IID_ENGINECAPABILITIES SL_IID_ENGINECAPABILITIES -- a
c-value SL_IID_THREADSYNC SL_IID_THREADSYNC -- a
