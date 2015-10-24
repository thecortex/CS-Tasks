﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Patterns_Recognition___Task_1
{
    class Task_2_View_Handler
    {
        const int default_width = 800;
        const int default_height = 600;

        ClassRegion[] array_class_regions;

        public void render_button_handler(Form parent_form, ComboBox comboBox_input_image_source, TextBox textBox_file_path, PictureBox pictureBox_input_image, TextBox textBox_width, TextBox textBox_height, DataGridView dataGridView_inputs, PictureBox pictureBox_greyscale_image, PictureBox pictureBox_classified)
        {
            int image_width = default_width;
            if (!String.IsNullOrEmpty(textBox_width.Text))
                image_width = Int32.Parse(textBox_width.Text);

            int image_height = default_height;
            if (!String.IsNullOrEmpty(textBox_height.Text))
                image_height = Int32.Parse(textBox_height.Text);

            Bitmap input_bmp = control_switch(parent_form, comboBox_input_image_source, textBox_file_path, pictureBox_input_image, image_width, image_height, dataGridView_inputs);
            textBox_width.Text = pictureBox_input_image.Image.Width.ToString();
            textBox_height.Text = pictureBox_input_image.Image.Height.ToString();
            Bitmap grey_image = new ImageProcessing().get_grey_scale(input_bmp);
            pictureBox_greyscale_image.Image = grey_image;
            propagata_class_regions_array(dataGridView_inputs, int.Parse(textBox_width.Text), int.Parse(textBox_height.Text));
            pictureBox_classified.Image = apply_bayesian_inference(grey_image, dataGridView_inputs);
        }

        public void propagata_class_regions_array(DataGridView data_meus_sigmas, int _width, int _height)
        {
            int num_rects = data_meus_sigmas.RowCount;
            if (num_rects == 1 && data_meus_sigmas.Rows[0].Cells[0].Value == null)
                return;
            array_class_regions = new ClassRegion[num_rects];
            int rect_width = _width / num_rects;
            for (int i = 0; i < num_rects; i++)
            {
                if (data_meus_sigmas.Rows[i].Cells[0].Value == null)
                    continue;

                array_class_regions[i] = new ClassRegion();
                array_class_regions[i].width = rect_width;
                array_class_regions[i].height = _height;
                array_class_regions[i].x1 = i * rect_width;
                array_class_regions[i].y1 = 0;
                array_class_regions[i].x2 = i * rect_width + rect_width;
                array_class_regions[i].y2 = _height;
                array_class_regions[i].meu_red = Double.Parse(data_meus_sigmas.Rows[i].Cells[0].Value.ToString());
                array_class_regions[i].meu_green = Double.Parse(data_meus_sigmas.Rows[i].Cells[2].Value.ToString());
                array_class_regions[i].meu_blue = Double.Parse(data_meus_sigmas.Rows[i].Cells[4].Value.ToString());
                array_class_regions[i].sigma_red = Double.Parse(data_meus_sigmas.Rows[i].Cells[1].Value.ToString());
                array_class_regions[i].sigma_green = Double.Parse(data_meus_sigmas.Rows[i].Cells[3].Value.ToString());
                array_class_regions[i].sigma_blue = Double.Parse(data_meus_sigmas.Rows[i].Cells[5].Value.ToString());
            }
        }

        public Bitmap control_switch(Form parent_form, ComboBox comboBox_input_image_source, TextBox textBox_file_path, PictureBox pictureBox_input_image, int image_width, int image_height, DataGridView dataGridView_inputs)
        {
            Bitmap input_bmp = null;
            switch (comboBox_input_image_source.SelectedIndex)
            {
                case 0:
                    input_bmp = upload_image(parent_form, textBox_file_path, pictureBox_input_image);
                    break;
                case 1:
                    input_bmp = generate_image(image_width, image_height, dataGridView_inputs, pictureBox_input_image);
                    break;
                default:
                    break;
            };
            return input_bmp;
        }

        public Bitmap generate_image(int width, int height, DataGridView data_meus_sigmas, PictureBox pict_box)
        {
            Bitmap bmp_image = new ImageRectangles().fill_image_rects(width, height, data_meus_sigmas, true);
            pict_box.Image = bmp_image;
            return bmp_image;
        }

        public Bitmap upload_image(Form parent_form, TextBox txtbox_file_path, PictureBox obj_picture_box)
        {
            return new ImageFilesHandler().ignite(parent_form, obj_picture_box, txtbox_file_path);
        }

        public Bitmap apply_bayesian_inference(Bitmap grey_image, DataGridView dgrv_meus_sigmas)
        {
            Bitmap ret = new Bitmap(grey_image.Width, grey_image.Height);
            for (int x = 0; x < grey_image.Width; x++)
            {
                for (int y = 0; y < grey_image.Height; y++)
                {
                    Color original_color = grey_image.GetPixel(x, y);
                    double averaged_color_value = (original_color.R + original_color.G + original_color.B) / 3;
                    int class_index = new BayesianInferenceEngine().classify(array_class_regions, averaged_color_value);
                    Color new_color = new ImageRectangles().get_random_color_intensity(dgrv_meus_sigmas.Rows[class_index]);
                    ret.SetPixel(x, y, new_color);
                }
            }
            return ret;
        }
    }
}
