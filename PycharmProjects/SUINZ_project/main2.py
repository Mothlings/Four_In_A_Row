import os

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'  # ignores info msgs
import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt
from tensorflow import keras
import pathlib
import shutil
from tensorflow.keras import datasets, layers, models

"""
## Generate a `Dataset`
"""

img_size = (160, 160)
num_classes = 4
batch_size = 32
image_size = (180, 180)

num_skipped = 0

start_dir_Gallery = pathlib.Path("Gallery")
Sorted_dir = pathlib.Path("Sorted")
data_dir = pathlib.Path("TrainingImages")

image_count = len(list(data_dir.glob('*/*.jpg')))
print(image_count)


def process_images_from_Gallery(num_skipped):
    for fname in os.listdir(start_dir_Gallery):
        fpath = os.path.join(start_dir_Gallery, fname)
        try:
            fobj = open(fpath, "rb")
            is_jfif = tf.compat.as_bytes("JFIF") in fobj.peek(10)
        finally:
            fobj.close()
        if not is_jfif:
            num_skipped += 1
            os.remove(fpath)


train_ds = tf.keras.preprocessing.image_dataset_from_directory(
    data_dir,
    validation_split=0.2,
    subset="training",
    seed=123,
    image_size=image_size,
    batch_size=batch_size,
)
val_ds = tf.keras.preprocessing.image_dataset_from_directory(
    data_dir,
    validation_split=0.2,
    subset="validation",
    seed=123,
    image_size=image_size,
    batch_size=batch_size,
)

class_names = train_ds.class_names
print(class_names)

data_augmentation = keras.Sequential(
    [
        layers.experimental.preprocessing.RandomFlip("horizontal"),
        layers.experimental.preprocessing.RandomRotation(0.1),
    ]
)

# Configure the dataset for performance
train_ds = train_ds.prefetch(buffer_size=32)
val_ds = val_ds.prefetch(buffer_size=32)


def visualize_image():  # 10 images
    plt.figure(figsize=(10, 10))
    for images, _ in train_ds.take(1):
        for i in range(9):
            augmented_images = data_augmentation(images)
            ax = plt.subplot(3, 3, i + 1)
            plt.imshow(augmented_images[0].numpy().astype("uint8"))
            #   plt.show()
            plt.axis("off")


## Build a model
def make_model(input_shape):
    inputs = keras.Input(shape=input_shape)
    # Image augmentation block
    x = data_augmentation(inputs)

    # Entry block
    x = layers.experimental.preprocessing.Rescaling(1.0 / 255)(x)
    x = layers.Conv2D(32, 3, strides=2, padding="same")(x)
    x = layers.BatchNormalization()(x)
    x = layers.Activation("relu")(x)

    x = layers.Conv2D(64, 3, padding="same")(x)
    x = layers.BatchNormalization()(x)
    x = layers.Activation("relu")(x)

    previous_block_activation = x  # Set aside residual

    for size in [128, 256, 512, 728]:
        x = layers.Activation("relu")(x)
        x = layers.SeparableConv2D(size, 3, padding="same")(x)
        x = layers.BatchNormalization()(x)

        x = layers.Activation("relu")(x)
        x = layers.SeparableConv2D(size, 3, padding="same")(x)
        x = layers.BatchNormalization()(x)

        x = layers.MaxPooling2D(3, strides=2, padding="same")(x)

        # Project residual
        residual = layers.Conv2D(size, 1, strides=2, padding="same")(
            previous_block_activation
        )
        x = layers.add([x, residual])  # Add back residual
        previous_block_activation = x  # Set aside next residual

    x = layers.SeparableConv2D(1024, 3, padding="same")(x)
    x = layers.BatchNormalization()(x)
    x = layers.Activation("relu")(x)

    x = layers.GlobalAveragePooling2D()(x)

    activation = "relu"
    units = 128

    x = layers.Dropout(0.5)(x)
    outputs = layers.Dense(units, activation=activation)(x)
    return keras.Model(inputs, outputs)


model = make_model(input_shape=image_size + (3,))
keras.utils.plot_model(model, show_shapes=True)
model.save('models')



def load_model(model):
    model = keras.models.load_model('models')
    keras.utils.plot_model(model, show_shapes=True)
    model.summary()


epochs = 8

callbacks = [
   keras.callbacks.ModelCheckpoint("save_at_{epoch}.h5"),
]
model.compile(optimizer='adam',
             loss=tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True),
             metrics=['accuracy'])
history = model.fit(
    train_ds, epochs=epochs, callbacks=callbacks, validation_data=val_ds,
)
acc = history.history['accuracy']
val_acc = history.history['val_accuracy']

loss = history.history['loss']
val_loss = history.history['val_loss']

epochs_range = range(epochs)

plt.figure(figsize=(8, 8))
plt.subplot(1, 2, 1)
plt.plot(epochs_range, acc, label='Training Accuracy')
plt.plot(epochs_range, val_acc, label='Validation Accuracy')
plt.legend(loc='lower right')
plt.title('Training and Validation Accuracy')

plt.subplot(1, 2, 2)
plt.plot(epochs_range, loss, label='Training Loss')
plt.plot(epochs_range, val_loss, label='Validation Loss')
plt.legend(loc='upper right')
plt.title('Training and Validation Loss')
plt.show()


for Image_name in os.listdir(start_dir_Gallery):
    Image_path = os.path.join(start_dir_Gallery, Image_name)
    img = keras.preprocessing.image.load_img(
        Image_path, target_size=image_size
    )
    img_array = keras.preprocessing.image.img_to_array(img)
    img_array = tf.expand_dims(img_array, 0)  # Create batch axis

    predictions = model.predict(img_array)
    score = tf.nn.softmax(predictions[0])
    ScorePrec = 100 * np.max(score)
    if class_names[np.argmax(score)] == "Food" and ScorePrec > 85:
        Image_new_path = os.path.join(Sorted_dir, "Food")
        shutil.move(Image_path, Image_new_path)
    elif class_names[np.argmax(score)] == "Nature" and ScorePrec > 85:
        Image_new_path = os.path.join(Sorted_dir, "Nature")
        shutil.move(Image_path, Image_new_path)
    elif class_names[np.argmax(score)] == "Pets" and ScorePrec > 85:
        Image_new_path = os.path.join(Sorted_dir, "Pets")
        shutil.move(Image_path, Image_new_path)
    elif class_names[np.argmax(score)] == "Selfies" and ScorePrec > 85:
        Image_new_path = os.path.join(Sorted_dir, "Selfie")
        shutil.move(Image_path, Image_new_path)
    else:
        Image_new_path = os.path.join(Sorted_dir, "Undefined")
        shutil.move(Image_path, Image_new_path)
    print(
        "This image most likely belongs to {} with a {:.2f} percent confidence."
            .format(class_names[np.argmax(score)], ScorePrec)
    )
