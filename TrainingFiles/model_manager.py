import tensorflow as tf

tf.logging.set_verbosity(tf.logging.ERROR)


class ModelManager:

    def __init__(self, load=False, num_input=32, num_views=100, num_output=121, learning_rate=0.001,
                 model_name="default_model", variation=0):
        self.learning_rate = learning_rate
        self.learning_decay = 0.995
        self.num_input = num_input
        self.num_views = num_views
        self.aux_info = num_views * 2  # Both current and visited
        self.num_output = num_output
        self.model_name = model_name
        self.folder = "Models/"
        self.activation_function = "sigmoid"
        self.load = load

        if load:
            self.model = self.load_model(model_name)
        else:
            if variation == 0:
                self.model = self.generate_conv_model()
            elif variation == 1:
                self.model = self.generate_first_variation_model()
            elif variation == 2:
                self.model = self.generate_second_variation_model()
            else:
                print("No valid model variation set")
                return
        self.compile_model()

    def get_model(self):
        return self.model

    def get_summary(self):
        a = []
        a.append("\n---------------------------- Model ----------------------")
        a.append("TensorFlow version: " + tf.__version__)
        a.append("TF Keras version: " + tf.keras.__version__)
        a.append("Model Loaded: {}".format(self.load))
        a.append("Learning Rate: {}".format(self.learning_rate))
        a.append("Activation Function: {}".format(self.activation_function))
        return a

    def generate_conv_model(self):

        inputs = tf.keras.layers.Input(shape=(32, 32, 32, 1), name="observations")
        c1 = tf.keras.layers.Conv3D(32, 5, 2, name="conv_layer_1")(inputs)
        c2 = tf.keras.layers.Conv3D(32, 3, 1, name="conv_layer_2")(c1)
        pool = tf.keras.layers.MaxPool3D(pool_size=(2, 2, 2), name="pooling_layer")(c2)
        conv_output = tf.keras.layers.Flatten(name="flatten_conv_output")(pool)

        fc1 = tf.keras.layers.Dense(128, activation="relu", name="fc_layer")(conv_output)

        auxiliary_inputs = tf.keras.layers.Input(shape=(self.aux_info, 1), name="views")
        aux_output = tf.keras.layers.Flatten(name="flatten_views")(auxiliary_inputs)
        aux_fcn = tf.keras.layers.Dense(100, activation="relu", name="aux_fc_layer")(aux_output)

        merged = tf.keras.layers.Concatenate()([fc1, aux_fcn])

        outputs = tf.keras.layers.Dense(self.num_views, activation=self.activation_function, name="actions")(merged)

        self.model = tf.keras.Model(inputs=[inputs, auxiliary_inputs], outputs=outputs)
        return self.model

    def generate_first_variation_model(self):

        inputs = tf.keras.layers.Input(shape=(32, 32, 32, 1), name="observations")
        c1 = tf.keras.layers.Conv3D(32, 5, 2, name="conv_layer_1")(inputs)
        c2 = tf.keras.layers.Conv3D(32, 3, 1, name="conv_layer_2")(c1)
        pool = tf.keras.layers.MaxPool3D(pool_size=(2, 2, 2), name="pooling_layer")(c2)
        conv_output = tf.keras.layers.Flatten(name="flatten_conv_output")(pool)

        fc1 = tf.keras.layers.Dense(128, activation="relu", name="fc_layer")(conv_output)

        auxiliary_inputs = tf.keras.layers.Input(shape=(self.aux_info, 1), name="views")
        aux_output = tf.keras.layers.Flatten(name="flatten_views")(auxiliary_inputs)
        aux_fcn = tf.keras.layers.Dense(100, activation="relu", name="aux_fc_layer")(aux_output)

        merged = tf.keras.layers.Concatenate()([fc1, aux_fcn])

        merged_fcn = tf.keras.layers.Dense(128, activation="relu", name="additional_fcn")(merged)

        outputs = tf.keras.layers.Dense(self.num_views, activation=self.activation_function, name="actions")(
            merged_fcn)

        self.model = tf.keras.Model(inputs=[inputs, auxiliary_inputs], outputs=outputs)
        return self.model

    def generate_second_variation_model(self):

        inputs = tf.keras.layers.Input(shape=(32, 32, 32, 1), name="observations")
        c1 = tf.keras.layers.Conv3D(32, 5, 2, name="conv_layer_1")(inputs)
        c2 = tf.keras.layers.Conv3D(32, 3, 1, name="conv_layer_2")(c1)
        pool = tf.keras.layers.MaxPool3D(pool_size=(2, 2, 2), name="pooling_layer")(c2)
        conv_output = tf.keras.layers.Flatten(name="flatten_conv_output")(pool)

        fc1 = tf.keras.layers.Dense(128, activation="relu", name="fc_layer")(conv_output)

        auxiliary_inputs = tf.keras.layers.Input(shape=(self.aux_info, 1), name="views")
        aux_output = tf.keras.layers.Flatten(name="flatten_views")(auxiliary_inputs)

        merged = tf.keras.layers.Concatenate()([fc1, aux_output])

        merged_fcn = tf.keras.layers.Dense(128, activation="relu", name="additional_fcn")(merged)

        outputs = tf.keras.layers.Dense(self.num_views, activation=self.activation_function, name="actions")(
            merged_fcn)

        self.model = tf.keras.Model(inputs=[inputs, auxiliary_inputs], outputs=outputs)
        return self.model

    def save_model(self, name="", store_binaries=False, keep_var_names=None, clear_devices=True):
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

        if store_binaries:
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
                frozen_graph = tf.compat.v1.graph_util.convert_variables_to_constants(
                    session, input_graph_def, output_names, freeze_var_names)

            tf.train.write_graph(frozen_graph, "", self.folder + name + ".bytes", as_text=False)
        print("Model saved as " + self.folder + name)
        # session.close()

    def load_model(self, model_name=""):
        if model_name == "":
            model_name = self.model_name
        loaded_model = tf.keras.models.load_model(model_name)
        self.model = loaded_model

        self.compile_model()

        print("Loaded Model -" + model_name)
        self.model.summary()
        return self.model

    def compile_model(self):
        opt = tf.keras.optimizers.Adam(lr=self.learning_rate)
        # self.model.compile(optimizer=tf.train.AdamOptimizer(self.learning_rate),
        self.model.compile(optimizer=opt,
                           loss='mean_squared_error',
                           metrics=['accuracy'])

    def _generate_auxiliary_input_model(self):

        print("TensorFlow version: " + tf.VERSION)
        print("TF Keras version: " + tf.keras.__version__)

        inputs = tf.keras.layers.Input(shape=(self.num_input,), name="observations")
        n1 = tf.keras.layers.Dense(200, activation="relu", name="hidden_layer_1")(inputs)

        auxiliary_inputs = tf.keras.layers.Input(shape=(self.num_input,), name="auxiliary_inputs")

        merged = tf.keras.layers.Concatenate()([n1, auxiliary_inputs])
        outputs = tf.keras.layers.Dense(self.num_output, activation="sigmoid", name="actions")(merged)

        self.model = tf.keras.Model(inputs=[inputs, auxiliary_inputs], outputs=outputs)

        self.compile_model()

        print("New Model Generated")
        print("Using Learning Rate: {}".format(self.learning_rate))
        print(self.model.summary())
        return self.model

    def print_model(self):
        self.model.summary()

    def decrement_learning_rate(self):
        self.learning_rate = self.learning_rate * self.learning_decay
        tf.keras.backend.set_value(self.model.optimizer.lr, self.learning_rate)
        # print("learning rate decayd to: {}".format(tf.keras.backend.eval(self.model.optimizer.lr)))


if __name__ == "__main__":
    print(tf.VERSION)
    print(tf.keras.__version__)
    import numpy as np

    print(tf.test.is_gpu_available())

    num_inputs = 32 * 32 * 32
    num_views = 242

    mm = ModelManager(model_name="generated_test_model")

    obs = np.random.randint(2, size=num_inputs).reshape((1, 32, 32, 32, 1))
    views = np.random.randint(2, size=num_views).reshape((1, 242, 1))

    p = mm.model.predict([obs, views])
    pmean = np.mean(p)
    p2std = np.std(p)

    obs2 = np.random.randint(1, size=num_inputs).reshape((1, 32, 32, 32, 1))
    views2 = np.random.randint(1, size=num_views).reshape((1, 242, 1))

    p2 = mm.model.predict([obs2, views2])
    p2mean = np.mean(p2)
    p2std = np.std(p2)

    tf.keras.backend.set_learning_phase(0)
    # tf.keras.backend.get_session(),
    mm.save_model()
    m = mm.load_model()
    # m.fit(data, labels, epochs=10, batch_size=32)
