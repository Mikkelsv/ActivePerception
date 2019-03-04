import tensorflow as tf
from tensorflow.python.keras import layers


class ModelManager:
    learning_rate = 0.01

    def __init__(self, load=False, num_input=2, num_output=4, learning_rate=0.01, model_name="default_model"):
        self.learning_rate = learning_rate
        self.num_input = num_input
        self.num_output = num_output
        self.model_name = model_name
        self.folder = "Models/"

        if load:
            self.model = self.load_model()
        else:
            self.model = self.generate_model()

    def get_model(self):
        return self.model

    def generate_model(self, print_version=True):
        """
            Generates cumstome made models specific for slam ball implementation, thus lacking in generatlization.
            Notice naming of 'observation' inputlayer and 'actions' output layer
        :param print_version: Print TF keras version number
        :param num_input: Number of inputs (observations) in model
        :param num_output: Number of outputs (action predictions) in model
        :return: TensorFlow Model
        """
        if print_version:
            print("TensorFlow version: " + tf.VERSION)
            print("TF Keras version: " + tf.keras.__version__)

        inputs = tf.keras.layers.Input(shape=(self.num_input,), name="observations")
        n1 = tf.keras.layers.Dense(6, activation="relu", name="hidden_layer_1")(inputs)
        n2 = tf.keras.layers.Dense(1028, activation="relu", name="hidden_layer_2")(n1)

        outputs = tf.keras.layers.Dense(self.num_output, activation="sigmoid", name="actions")(n1)

        self.model = tf.keras.Model(inputs=inputs, outputs=outputs)

        self._compile_model()

        print("New Model Generated")
        print("Using Learning Rate: {}".format(self.learning_rate))
        print(self.model.summary())
        return self.model

    def save_model(self, name="", keep_var_names=None, clear_devices=True):
        """
            Freezes the state of a session into a pruned computation graph.
            Commonly called through  --- save_session(tf.keras.backend.get_session()) ----

            Creates a new computation graph where variable nodes are replaced by
            constants taking their current value in the session. The new graph will be
            pruned so subgraphs that are not necessary to compute the requested
            outputs are removed.
            @param session The TensorFlow session to be frozen.
            @param keep_var_names A list of variable names that should not be frozen,
                                or None to freeze all the variables in the graph.
            @param output_names Names of the relevant graph outputs.
            @param clear_devices Remove the device directives from the graph for better portability.
            @return The frozen graph definition.
            :param name: Name of savepath
          """
        if name == "":
            name = self.model_name

        output_names = [out.op.name for out in self.model.outputs]

        self.model.save(self.folder + name + ".h5")

        session = tf.keras.backend.get_session()
        graph = session.graph
        with graph.as_default():
            freeze_var_names = list(
                set(v.op.name for v in tf.global_variables()).difference(keep_var_names or []))
            output_names = output_names or []
            output_names += [v.op.name for v in tf.global_variables()]
            input_graph_def = graph.as_graph_def()
            if clear_devices:
                for node in input_graph_def.node:
                    node.device = ""
            frozen_graph = tf.graph_util.convert_variables_to_constants(
                session, input_graph_def, output_names, freeze_var_names)

        tf.train.write_graph(frozen_graph, "",self.folder + name + ".bytes", as_text=False)
        print("Model saved as " + self.folder + name)

    def load_model(self, model_name=""):
        if model_name == "":
            model_name = self.model_name
        loaded_model = tf.keras.models.load_model(self.folder + model_name + ".h5")
        self.model = loaded_model

        self._compile_model()

        print("Loaded Model -" + model_name + ".h5- from " + self.folder)
        return self.model

    def _compile_model(self):
        self.model.compile(optimizer=tf.train.AdamOptimizer(self.learning_rate),
                           loss='categorical_crossentropy',
                           metrics=['accuracy'])


if __name__ == "__main__":
    print(tf.VERSION)
    print(tf.keras.__version__)
    import numpy as np

    print(tf.test.is_gpu_available())

    num_inputs = 2
    num_outputs = 2
    mm = ModelManager(False, num_inputs, num_outputs, model_name="generated_test_model")

    data = np.random.random((1000, num_inputs))
    labels = np.random.random((1000, num_outputs))

    mm.model.fit(data, labels, epochs=10, batch_size=32)

    tf.keras.backend.set_learning_phase(0)
    # tf.keras.backend.get_session(),
    mm.save_model()
    m = mm.load_model()
    # m.fit(data, labels, epochs=10, batch_size=32)